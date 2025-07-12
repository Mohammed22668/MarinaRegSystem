using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("LabInventory")]
    public class LabInventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("LabTest")]
        public int LabTestId { get; set; }

        public virtual LabTest LabTest { get; set; }

        [Display(Name = "الكمية المتوفرة")]
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Display(Name = "تاريخ آخر تحديث")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
