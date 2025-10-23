using SqlSugar;

namespace WorkFlowDemo.Models.Dtos
{
    public class CreateMaterialDto
    {
        public string Id { get; set; }

        public string BatchNumber { get; set; }

        public string MaterialCode { get; set; }

        public decimal Qty { get; set; }

        public DateTime OperationTime { get; set; }

        public string Operator { get; set; }
    }
}