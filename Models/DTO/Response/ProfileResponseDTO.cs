using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Models.DTO.Response
{
    public class ProfileResponseDTO
    {
        public string? AccountId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? RoleId { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public int? FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
