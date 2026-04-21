using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SocietyManagementSystem.Models
{
    public enum RentStatus
    {
        Pending,
        Paid,
        Overdue
    }

    public class Rent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("tenantId")]
        [Required]
        public string? TenantId { get; set; }

        [BsonElement("amount")]
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [BsonElement("dueDate")]
        [Required]
        public DateTime DueDate { get; set; }

        [BsonElement("paidDate")]
        public DateTime? PaidDate { get; set; }

        [BsonElement("status")]
        public RentStatus Status { get; set; } = RentStatus.Pending;

        [BsonElement("month")]
        public int Month { get; set; }

        [BsonElement("year")]
        public int Year { get; set; }
    }
}