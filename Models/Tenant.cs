using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocietyManagementSystem.Models
{
    public class Tenant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string? Title { get; set; }

        [BsonElement("firstName")]
        public string? FirstName { get; set; }

        [BsonElement("middleName")]
        public string? MiddleName { get; set; }

        [BsonElement("lastName")]
        public string? LastName { get; set; }

        [BsonElement("gender")]
        public string? Gender { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [BsonElement("anniversary")]
        public DateTime? Anniversary { get; set; }
    }
}