namespace WorkFlowDemo.Models.Dtos
{
    public class ScanDto
    {
        public string BatchNumber { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public decimal Qty { get; set; }
    }
}