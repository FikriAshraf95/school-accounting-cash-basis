namespace SchoolAccounting.Api.Features.Categories;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // income / expense
    public int LedgerId { get; set; }
    public string? Description { get; set; }
    public bool RequiresStudent { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public int LedgerId { get; set; }
    public string? Description { get; set; }
    public bool RequiresStudent { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int LedgerId { get; set; }
    public string LedgerName { get; set; } = string.Empty;
    public string LedgerCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresStudent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
