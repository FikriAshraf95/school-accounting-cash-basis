using Microsoft.EntityFrameworkCore;
using SchoolAccounting.Api.Common.Exceptions;
using SchoolAccounting.Api.Infrastructure.Persistence;

namespace SchoolAccounting.Api.Features.BusinessInfo;

public class BusinessInfoService
{
    private readonly AppDbContext _dbContext;

    public BusinessInfoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BusinessInfoResponse> GetBusinessInfoAsync()
    {
        var businessInfo = await _dbContext.BusinessInfos
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Business information not found");

        return businessInfo.ToResponse();
    }

    public async Task<BusinessInfoResponse> UpdateBusinessInfoAsync(UpdateBusinessInfoRequest request)
    {
        var businessInfo = await _dbContext.BusinessInfos
            .FirstOrDefaultAsync();

        if (businessInfo == null)
        {
            // Create new business info if none exists
            businessInfo = new Infrastructure.Persistence.BusinessInfo
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.BusinessInfos.Add(businessInfo);
        }
        else
        {
            businessInfo.UpdatedAt = DateTime.UtcNow;
        }

        businessInfo.SchoolName = request.SchoolName;
        businessInfo.RegistrationNumber = request.RegistrationNumber;
        businessInfo.Address = request.Address;
        businessInfo.Phone = request.Phone;
        businessInfo.Email = request.Email;
        businessInfo.FinancialYearStart = request.FinancialYearStart;
        businessInfo.FinancialYearEnd = request.FinancialYearEnd;
        businessInfo.Currency = request.Currency ?? "PHP";
        businessInfo.Timezone = request.Timezone;
        businessInfo.BankName = request.BankName;
        businessInfo.BankAccountName = request.BankAccountName;
        businessInfo.BankAccountNumber = request.BankAccountNumber;
        businessInfo.TaxRegistration = request.TaxRegistration;
        businessInfo.TaxRate = request.TaxRate;

        await _dbContext.SaveChangesAsync();

        return businessInfo.ToResponse();
    }
}

public static class BusinessInfoMappings
{
    public static BusinessInfoResponse ToResponse(this Infrastructure.Persistence.BusinessInfo businessInfo)
    {
        return new BusinessInfoResponse
        {
            Id = businessInfo.Id,
            SchoolName = businessInfo.SchoolName,
            RegistrationNumber = businessInfo.RegistrationNumber,
            Address = businessInfo.Address,
            Phone = businessInfo.Phone,
            Email = businessInfo.Email,
            FinancialYearStart = businessInfo.FinancialYearStart,
            FinancialYearEnd = businessInfo.FinancialYearEnd,
            Currency = businessInfo.Currency,
            Timezone = businessInfo.Timezone,
            BankName = businessInfo.BankName,
            BankAccountName = businessInfo.BankAccountName,
            BankAccountNumber = businessInfo.BankAccountNumber,
            TaxRegistration = businessInfo.TaxRegistration,
            TaxRate = businessInfo.TaxRate,
            CreatedAt = businessInfo.CreatedAt,
            UpdatedAt = businessInfo.UpdatedAt
        };
    }
}
