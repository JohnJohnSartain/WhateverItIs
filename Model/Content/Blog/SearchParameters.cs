namespace Model.Content.Blog;

public record SearchParameters : BaseSearchParameters
{
    public string? ArticleId { get; set; }
    public string? CreatorId { get; set; }
    public bool? SortByMostPopular { get; set; }
    public bool? SortByMostViewed { get; set; }
    public string? Tag { get; set; }
}