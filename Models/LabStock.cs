using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("LabStocks")]
    public class LabStock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("LabTest")]
        public int LabTestId { get; set; }

        public virtual LabTest LabTest { get; set; }

        [Display(Name = "الكمية المتوفرة")]
        [Range(0, double.MaxValue)]
        public decimal QuantityAvailable { get; set; }

        [Display(Name = "تاريخ الإضافة")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
