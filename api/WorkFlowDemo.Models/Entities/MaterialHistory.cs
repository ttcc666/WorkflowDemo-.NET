using SqlSugar;
using System;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("MaterialHistory")]
    public class MaterialHistory
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; } = string.Empty;

        [SugarColumn(ColumnName = "BatchNumber")]
        public string BatchNumber { get; set; } = string.Empty;

        [SugarColumn(ColumnName = "MaterialCode")]
        public string MaterialCode { get; set; } = string.Empty;

        public decimal Qty { get; set; }

        [SugarColumn(ColumnName = "OperationTime")]
        public DateTime OperationTime { get; set; }

        [SugarColumn(ColumnName = "Operator")]
        public string Operator { get; set; } = string.Empty;


        [SugarColumn(ColumnName = "CreatimeTime")]
        public DateTime CreatimeTime { get; set; }
    }
}
