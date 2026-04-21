using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SocietyManagementSystem.Models
{
    public enum MaintenanceStatus
    {
        Open,
        InProgress,
        Closed
    }

    public enum MaintenancePriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum MaintenanceCategory
    {
        Plumbing,
        Electrical,
        Carpentry,
        Painting,
        Cleaning,
        Security,
        Other
    }

    public class MaintenanceRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("tenantId")]
        [Required]
        public string? TenantId { get; set; }

        [BsonElement("title")]
        [Required]
        [StringLength(100)]
        public string? Title { get; set; }

        [BsonElement("description")]
        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        [BsonElement("category")]
        public MaintenanceCategory Category { get; set; }

        [BsonElement("priority")]
        public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;

        [BsonElement("status")]
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Open;

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [BsonElement("resolvedDate")]
        public DateTime? ResolvedDate { get; set; }
    }
}