using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarinaRegSystem.Models
{
    [Table("LabTestCategories")]
    public class LabTestCategory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم التصنيف")]
        public string Name { get; set; }

        // علاقات
        public virtual ICollection<LabTest> LabTests { get; set; }

        public LabTestCategory()
        {
            LabTests = new HashSet<LabTest>();
        }
    }
}
