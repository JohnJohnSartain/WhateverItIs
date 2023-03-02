using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Model.Content.Blog;

public class ArticleView : Base
{
    [Required][BsonRepresentation(BsonType.ObjectId)] public string ArticleId { get; set; }
}