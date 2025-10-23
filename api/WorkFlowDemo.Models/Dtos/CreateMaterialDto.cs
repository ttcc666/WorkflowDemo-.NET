using SqlSugar;

namespace WorkFlowDemo.Models.Dtos
{
    public class CreateMaterialDto
    {
        public string Id { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        public string MaterialCode { get; set; } = string.Empty;

        public decimal Qty { get; set; }

        public DateTime OperationTime { get; set; }

        public string Operator { get; set; } = string.Empty;
    }
}