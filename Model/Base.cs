using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Model;

public abstract class Base
{
    [BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    [Required] public DateTime Created { get; set; }
    [Required][BsonRepresentation(BsonType.ObjectId)] public string CreatedBy { get; set; }
    [Required] public DateTime Modified { get; set; }
    [Required][BsonRepresentation(BsonType.ObjectId)] public string ModifiedBy { get; set; }
}