using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.UserManagement;

public class UserManagementService
{
    private readonly AppDbContext _dbContext;

    public UserManagementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<UserSummaryResponse>> GetUsersAsync(int page, int perPage, string? search)
    {
        var query = _dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(searchLower) ||
                u.Username.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        var total = await query.CountAsync();
        var lastPage = (int)Math.Ceiling((double)total / perPage);

        var users = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return new PagedResult<UserSummaryResponse>
        {
            Data = users.Select(u => u.ToSummaryResponse()).ToList(),
            Meta = new PagedResultMeta
            {
                Total = total,
                Page = page,
                PerPage = perPage,
                LastPage = lastPage
            }
        };
    }

    public async Task<UserSummaryResponse> GetUserAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        return user.ToSummaryResponse();
    }

    public async Task<UserSummaryResponse> CreateUserAsync(CreateUserRequest request)
    {
        // Check if email already exists
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new ConflictException("Email already registered");
        }

        // Check if username already exists
        if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
        {
            throw new ConflictException("Username already taken");
        }

        // Validate role
        if (!IsValidRole(request.Role))
        {
            throw new BusinessRuleException($"Invalid role: {request.Role}");
        }

        var user = new User
        {
            Name = request.Name,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user.ToSummaryResponse();
    }

    public async Task<UserSummaryResponse> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _dbContext.Users.FindAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        // Check if email is being changed and if it's already taken
        if (user.Email != request.Email && await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
        {
            throw new ConflictException("Email already registered");
        }

        // Check if username is being changed and if it's already taken
        if (user.Username != request.Username && await _dbContext.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
        {
            throw new ConflictException("Username already taken");
        }

        user.Name = request.Name;
        user.Username = request.Username;
        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return user.ToSummaryResponse();
    }

    public async Task<UserSummaryResponse> AssignRoleAsync(int id, AssignRoleRequest request)
    {
        // Validate role
        if (!IsValidRole(request.Role))
        {
            throw new BusinessRuleException($"Invalid role: {request.Role}");
        }

        var user = await _dbContext.Users.FindAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        // Last-admin guard: prevent removing the last admin
        if (user.Role == AppRole.Admin && request.Role != AppRole.Admin)
        {
            var adminCount = await _dbContext.Users.CountAsync(u => u.Role == AppRole.Admin);
            if (adminCount <= 1)
            {
                throw new BusinessRuleException("Cannot change role of the last admin user");
            }
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return user.ToSummaryResponse();
    }

    public async Task ChangePasswordAsync(int id, ChangePasswordRequest request)
    {
        var user = await _dbContext.Users.FindAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        // Last-admin guard: prevent deleting the last admin
        if (user.Role == AppRole.Admin)
        {
            var adminCount = await _dbContext.Users.CountAsync(u => u.Role == AppRole.Admin);
            if (adminCount <= 1)
            {
                throw new BusinessRuleException("Cannot delete the last admin user");
            }
        }

        // Revoke all tokens for this user
        var tokens = await _dbContext.PersonalAccessTokens
            .Where(t => t.UserId == id)
            .ToListAsync();
        
        _dbContext.PersonalAccessTokens.RemoveRange(tokens);
        _dbContext.Users.Remove(user);
        
        await _dbContext.SaveChangesAsync();
    }

    private static bool IsValidRole(string role)
    {
        return role == AppRole.Admin ||
               role == AppRole.Accountant ||
               role == AppRole.Staff ||
               role == AppRole.Viewer;
    }
}

public static class UserManagementMappings
{
    public static UserSummaryResponse ToSummaryResponse(this User user)
    {
        return new UserSummaryResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
