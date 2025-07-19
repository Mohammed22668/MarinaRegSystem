

namespace MarinaRegSystem.Models
{
    public class LabTestWithStockViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal UsagePerPatient { get; set; }
        public int? LabTestCategoryId { get; set; }
        public decimal StockQuantity { get; set; }
    }



}