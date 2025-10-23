using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class Transaction : BaseModel
    {
        // TransactionID INT PRIMARY KEY IDENTITY(1,1)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        // UserID INT FOREIGN KEY REFERENCES Users(UserID)
        public int UserId { get; set; }

        // TransactionType VARCHAR(50) NOT NULL
        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        // Amount DECIMAL(18, 2) NOT NULL
        // Dùng kiểu 'decimal' trong C# để đại diện cho DECIMAL(18, 2)
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        // TransactionDate DATETIME DEFAULT GETDATE()
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Status VARCHAR(50) NOT NULL
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        // PaymentMethod NVARCHAR(50)
        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Cho phép null

        // ReferenceID INT
        public int? ReferenceId { get; set; } // Cho phép null

        // Thuộc tính điều hướng (Navigation Property)
        // Liên kết Transaction với User
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = default!;
    }
}
