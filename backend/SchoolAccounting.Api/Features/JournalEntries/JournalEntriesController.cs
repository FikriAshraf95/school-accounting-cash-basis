using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.JournalEntries;

[ApiController]
[Route("api/v1/journal-entries")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly JournalEntryService _journalEntryService;

    public JournalEntriesController(JournalEntryService journalEntryService)
    {
        _journalEntryService = journalEntryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<JournalEntryResponse>>> GetJournalEntries(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 15,
        [FromQuery] int? ledgerId = null,
        [FromQuery] string? journalType = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var result = await _journalEntryService.GetJournalEntriesAsync(page, perPage, ledgerId, journalType, dateFrom, dateTo);
        return Ok(result);
    }
}
