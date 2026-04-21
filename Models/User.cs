using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SocietyManagementSystem.Models
{
    public enum UserRole
    {
        Admin,
        Tenant
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        [Required]
        public string? Username { get; set; }

        [BsonElement("password")]
        [Required]
        public string? Password { get; set; }

        [BsonElement("email")]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [BsonElement("role")]
        public UserRole Role { get; set; } = UserRole.Admin;
    }
}