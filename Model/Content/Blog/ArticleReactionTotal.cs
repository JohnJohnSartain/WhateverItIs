using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Model.Content.Blog;

public class ArticleReactionTotal : Base
{
    [Required][BsonRepresentation(BsonType.ObjectId)] public string ArticleId { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
}