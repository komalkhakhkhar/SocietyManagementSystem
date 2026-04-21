using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SocietyManagementSystem.Models
{
    public enum NoticeAudience
    {
        All,
        Tenants,
        Admins
    }

    public class Notice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [BsonElement("content")]
        [Required]
        [StringLength(2000)]
        public string? Content { get; set; }

        [BsonElement("postedBy")]
        [Required]
        public string? PostedBy { get; set; } // User Id

        [BsonElement("postedDate")]
        public DateTime PostedDate { get; set; } = DateTime.Now;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("audience")]
        public NoticeAudience Audience { get; set; } = NoticeAudience.All;
    }
}