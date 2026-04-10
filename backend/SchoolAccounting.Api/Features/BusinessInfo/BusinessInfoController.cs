using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.BusinessInfo;

[ApiController]
[Route("api/v1/business-info")]
[Authorize]
public class BusinessInfoController : ControllerBase
{
    private readonly BusinessInfoService _businessInfoService;

    public BusinessInfoController(BusinessInfoService businessInfoService)
    {
        _businessInfoService = businessInfoService;
    }

    [HttpGet]
    public async Task<ActionResult<BusinessInfoResponse>> GetBusinessInfo()
    {
        var businessInfo = await _businessInfoService.GetBusinessInfoAsync();
        return Ok(businessInfo);
    }

    [HttpPut]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<BusinessInfoResponse>> UpdateBusinessInfo(UpdateBusinessInfoRequest request)
    {
        var businessInfo = await _businessInfoService.UpdateBusinessInfoAsync(request);
        return Ok(businessInfo);
    }
}
