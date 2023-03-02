using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Model.Content.Blog;

public class ArticleReaction : Base
{
    [Required][BsonRepresentation(BsonType.ObjectId)] public string ArticleId { get; set; }
    public bool? IsLiked { get; set; }
    public bool? IsDisliked { get; set; }
}