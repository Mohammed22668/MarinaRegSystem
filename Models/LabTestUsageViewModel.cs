namespace MarinaRegSystem.Models
{
    public class LabTestUsageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal Price { get; set; }
        public decimal UsagePerPatient { get; set; }
        public decimal StockQuantity { get; set; }
        public string LabTestCategoryName { get; set; }
        public int UsageCount { get; set; }
    }
}