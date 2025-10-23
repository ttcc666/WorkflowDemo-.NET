namespace WorkFlowDemo.Models.Dtos
{
    public class AddInventoryDto
    {
        public string MaterialCode { get; set; } = string.Empty;
        public decimal Qty { get; set; }
    }
}