using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models.Model
{
    public enum OperateStatus
    {
        Online,
        Offline
    }
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AccountId { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Avatar { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public OperateStatus Status { get; set; } = OperateStatus.Offline;
        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoleId { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public int? FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
