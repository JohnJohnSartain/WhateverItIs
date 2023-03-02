namespace Model.Content.Blog;

public class ArticlePublic : Article
{
    public ArticlePublic(Article article)
    {
        Id = article.Id;
        Created = article.Created;
        CreatedBy = article.CreatedBy;
        Modified = article.Modified;
        ModifiedBy = article.ModifiedBy;
        Name = article.Name;
        Description = article.Description;
        Content = article.Content;
        Tags = article.Tags;
        ImageUrlHeader = article.ImageUrlHeader;
        ImageUrlThumbnail = article.ImageUrlThumbnail;
        IsDeleted = article.IsDeleted;
    }

    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public int ApprovalRating { get => (Likes == 0 || Dislikes == 0) ? 100 : (int)(((double)Likes / ((double)Likes + (double)Dislikes)) * 100); }
    public string CreatorName { get; set; }
    public string CreatorProfileImageUrl { get; set; }
    public int Views { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsDisliked { get; set; }
}