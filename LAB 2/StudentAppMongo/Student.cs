using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentAppMongo;

public class Student
{
    // MongoDB tự sinh _id (ObjectId, 24 ký tự hex) khi insert
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public int Age { get; set; }
    public double Grade { get; set; }

    public override string ToString()
        => $"{Id,-25}{Name,-22} {Email,-26} {Address,-16} {Age,-4} {Grade,-5:0.0#}";
}
