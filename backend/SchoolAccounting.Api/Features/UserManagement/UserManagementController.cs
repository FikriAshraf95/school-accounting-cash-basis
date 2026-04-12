using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.UserManagement;

[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = AppRole.Admin)]
public class UserManagementController : ControllerBase
{
    private readonly UserManagementService _userManagementService;

    public UserManagementController(UserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserSummaryResponse>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 10,
        [FromQuery] string? search = null)
    {
        var result = await _userManagementService.GetUsersAsync(page, perPage, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserSummaryResponse>> GetUser(int id)
    {
        var user = await _userManagementService.GetUserAsync(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserSummaryResponse>> CreateUser(CreateUserRequest request)
    {
        var user = await _userManagementService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserSummaryResponse>> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await _userManagementService.UpdateUserAsync(id, request);
        return Ok(user);
    }

    [HttpPut("{id}/role")]
    public async Task<ActionResult<UserSummaryResponse>> AssignRole(int id, AssignRoleRequest request)
    {
        var user = await _userManagementService.AssignRoleAsync(id, request);
        return Ok(user);
    }

    [HttpPut("{id}/password")]
    public async Task<ActionResult> ChangePassword(int id, ChangePasswordRequest request)
    {
        await _userManagementService.ChangePasswordAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        await _userManagementService.DeleteUserAsync(id);
        return NoContent();
    }
}
