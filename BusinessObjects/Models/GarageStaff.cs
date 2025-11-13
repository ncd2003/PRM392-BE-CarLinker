using BusinessObjects.Models.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    /// <summary>
    /// GarageStaff - Nhân viên của Garage
    /// Roles: DEALER, WAREHOUSE, STAFF (không bao gồm GARAGE owner)
    /// </summary>
    public class GarageStaff : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? Image { get; set; }

        /// <summary>
        /// Role của nhân viên: DEALER(0), WAREHOUSE(1), STAFF(2)
        /// </summary>
        public RoleGarage GarageRole { get; set; } = RoleGarage.STAFF;
        
        public UserStatus UserStatus { get; set; } = UserStatus.ACTIVE;

        [MaxLength(500)]
        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        public bool IsActive { get; set; } = true;

        [Required]
        // Foreign Key to Garage
        [ForeignKey(nameof(GarageId))]
        public int GarageId { get; set; }
        
        // Navigation Property
        public virtual Garage Garage { get; set; } = default!;
    }
}
