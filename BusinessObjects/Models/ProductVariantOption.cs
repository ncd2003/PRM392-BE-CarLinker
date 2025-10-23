using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ProductVariantOption : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("ProductVariant")]
        public int VariantId { get; set; }

        [Required]
        [ForeignKey("OptionValue")]
        public int OptionValueId { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }
        public virtual OptionValue OptionValue { get; set; }
    }
}
