namespace SchoolAccounting.Api.Common;

public class PagedResult<T>
{
    public List<T> Data { get; set; } = [];
    public PagedResultMeta Meta { get; set; } = new();
}

public class PagedResultMeta
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int LastPage { get; set; }
}
