namespace VmesteGO.Dto.Responses;

public record PagedResponse<T>(IEnumerable<T> Items, int TotalCount);