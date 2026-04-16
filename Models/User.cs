using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocietyManagementSystem.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("password")]
        public string? Password { get; set; } // In real app, hash the password

        [BsonElement("email")]
        public string? Email { get; set; }
    }
}