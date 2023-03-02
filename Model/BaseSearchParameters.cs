namespace Model;

public abstract record BaseSearchParameters
{
    public int? Quantity { get; set; }
    public bool? SortByAscending { get; set; }
    public bool? SortByDescending { get; set; }
    public string? UserId { get; set; }
    public string[]? Ids { get; set; }
    public string? Token { get; set; }
}