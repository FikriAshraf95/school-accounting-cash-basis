using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.Ledgers;

[ApiController]
[Route("api/v1/ledgers")]
[Authorize]
public class LedgersController : ControllerBase
{
    private readonly LedgerService _ledgerService;

    public LedgersController(LedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<LedgerResponse>>> GetLedgers(
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _ledgerService.GetLedgersAsync(type, isActive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LedgerResponse>> GetLedger(int id)
    {
        var ledger = await _ledgerService.GetLedgerAsync(id);
        return Ok(ledger);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<LedgerResponse>> CreateLedger(CreateLedgerRequest request)
    {
        var ledger = await _ledgerService.CreateLedgerAsync(request);
        return CreatedAtAction(nameof(GetLedger), new { id = ledger.Id }, ledger);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<LedgerResponse>> UpdateLedger(int id, UpdateLedgerRequest request)
    {
        var ledger = await _ledgerService.UpdateLedgerAsync(id, request);
        return Ok(ledger);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult> DeleteLedger(int id)
    {
        await _ledgerService.DeleteLedgerAsync(id);
        return NoContent();
    }
}
