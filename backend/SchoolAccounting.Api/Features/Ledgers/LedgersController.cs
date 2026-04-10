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
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 15,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _ledgerService.GetLedgersAsync(page, perPage, sortBy, sortDesc, type, isActive);
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

    [HttpGet("reports/trial-balance")]
    public async Task<ActionResult<TrialBalanceResponse>> GetTrialBalance(
        [FromQuery] int? year = null)
    {
        var result = await _ledgerService.GetTrialBalanceAsync(year);
        return Ok(result);
    }

    [HttpGet("summary/{year}")]
    public async Task<ActionResult<LedgerSummaryResponse>> GetLedgerSummary(int year)
    {
        var result = await _ledgerService.GetLedgerSummaryByYearAsync(year);
        return Ok(result);
    }

    [HttpPost("year-end-close")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<YearEndCloseResponse>> YearEndClose(YearEndCloseRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _ledgerService.YearEndCloseAsync(request.Year, userId);
        return Ok(result);
    }

    [HttpPost("year-beginning-open")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<YearBeginningOpenResponse>> YearBeginningOpen(YearBeginningOpenRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _ledgerService.YearBeginningOpenAsync(request.Year, userId);
        return Ok(result);
    }
}
