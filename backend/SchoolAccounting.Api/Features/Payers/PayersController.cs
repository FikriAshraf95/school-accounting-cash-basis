using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.Payers;

[ApiController]
[Route("api/v1/payers")]
[Authorize]
public class PayersController : ControllerBase
{
    private readonly PayerService _payerService;

    public PayersController(PayerService payerService)
    {
        _payerService = payerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PayerResponse>>> GetPayers(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 15,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _payerService.GetPayersAsync(page, perPage, sortBy, sortDesc, search, type, isActive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PayerDetailResponse>> GetPayer(int id)
    {
        var payer = await _payerService.GetPayerAsync(id);
        return Ok(payer);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant},{AppRole.Staff}")]
    public async Task<ActionResult<PayerResponse>> CreatePayer(CreatePayerRequest request)
    {
        var payer = await _payerService.CreatePayerAsync(request);
        return CreatedAtAction(nameof(GetPayer), new { id = payer.Id }, payer);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant},{AppRole.Staff}")]
    public async Task<ActionResult<PayerResponse>> UpdatePayer(int id, UpdatePayerRequest request)
    {
        var payer = await _payerService.UpdatePayerAsync(id, request);
        return Ok(payer);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult> DeletePayer(int id)
    {
        await _payerService.DeletePayerAsync(id);
        return NoContent();
    }
}
