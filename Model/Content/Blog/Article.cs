using System.ComponentModel.DataAnnotations;

namespace Model.Content.Blog;

public class Article : Base
{
    [Required]
    [MinLength(1)]
    public string Name { get; set; }

    [Required]
    [MinLength(1)]
    public string Description { get; set; }

    [Required]
    [MinLength(50)]
    public string Content { get; set; }

    [MaxLength(5)]
    public string[] Tags { get; set; }

    public string ImageUrlHeader { get; set; }

    public string ImageUrlThumbnail { get; set; }

    public bool IsDeleted { get; set; }
}