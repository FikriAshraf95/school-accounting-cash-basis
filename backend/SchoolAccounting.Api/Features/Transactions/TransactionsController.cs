using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;
using System.Security.Claims;

namespace SchoolAccounting.Api.Features.Transactions;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;
    private readonly TransactionReversalService _reversalService;

    public TransactionsController(
        TransactionService transactionService,
        TransactionReversalService reversalService)
    {
        _transactionService = transactionService;
        _reversalService = reversalService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TransactionListItemResponse>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 15,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? transactableType = null,
        [FromQuery] int? transactableId = null)
    {
        var result = await _transactionService.GetTransactionsAsync(page, perPage, sortBy, sortDesc, type, dateFrom, dateTo, transactableType, transactableId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponse>> GetTransaction(int id)
    {
        var transaction = await _transactionService.GetTransactionAsync(id);
        return Ok(transaction);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant},{AppRole.Staff}")]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction(CreateTransactionRequest request)
    {
        var userId = GetCurrentUserId();
        var transaction = await _transactionService.CreateTransactionAsync(request, userId);
        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant},{AppRole.Staff}")]
    public async Task<ActionResult<TransactionResponse>> UpdateTransaction(int id, UpdateTransactionRequest request)
    {
        var userId = GetCurrentUserId();
        var transaction = await _transactionService.UpdateTransactionAsync(id, request, userId);
        return Ok(transaction);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult> DeleteTransaction(int id)
    {
        await _transactionService.DeleteTransactionAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/reverse")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<TransactionResponse>> ReverseTransaction(int id)
    {
        var userId = GetCurrentUserId();
        var reversal = await _reversalService.ReverseTransactionAsync(id, userId);
        return Ok(reversal);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
