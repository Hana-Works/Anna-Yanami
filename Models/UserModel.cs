namespace AnnaYanami.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set;} = ObjectId.GenerateNewId().ToString();

    [BsonElement("uid")]
    public ulong Uid { get; set; }

    [BsonElement("username")]
    public string Username { get; private set; } = "User" + new Random().Next(1000, 9999).ToString();

    [BsonElement("quota")]
    public int Quota { get; set; } = 0;

    [BsonElement("is_banned")]
    public bool IsBanned { get; set; } = false;
}