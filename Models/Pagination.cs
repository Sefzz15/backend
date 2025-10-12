namespace backend.Models;

public sealed class PaginatedResponse<T>
{
    public required IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    public required int TotalItems { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
}
