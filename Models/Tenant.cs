using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SocietyManagementSystem.Models
{
    public enum TenantStatus
    {
        Active,
        Inactive,
        Evicted
    }

    public class Tenant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        [Required]
        public string? Title { get; set; }

        [BsonElement("firstName")]
        [Required]
        public string? FirstName { get; set; }

        [BsonElement("middleName")]
        public string? MiddleName { get; set; }

        [BsonElement("lastName")]
        [Required]
        public string? LastName { get; set; }

        [BsonElement("gender")]
        [Required]
        public string? Gender { get; set; }

        [BsonElement("email")]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [BsonElement("phone")]
        [Required]
        [Phone]
        public string? Phone { get; set; }

        [BsonElement("unitNumber")]
        public string? UnitNumber { get; set; }

        [BsonElement("status")]
        public TenantStatus Status { get; set; } = TenantStatus.Active;

        [BsonElement("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [BsonElement("anniversary")]
        public DateTime? Anniversary { get; set; }
    }
}