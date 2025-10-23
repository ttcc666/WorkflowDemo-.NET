using SqlSugar;
using System;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("MaterialTemporaryScan")]
    public class MaterialTemporaryScan
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        [SugarColumn(ColumnName = "BatchNumber")]
        public string BatchNumber { get; set; }

        [SugarColumn(ColumnName = "MaterialCode")]
        public string MaterialCode { get; set; }

        public decimal Qty { get; set; }

        [SugarColumn(ColumnName = "OperationTime")]
        public DateTime OperationTime { get; set; }

        [SugarColumn(ColumnName = "Operator")]
        public string Operator { get; set; }
    }
}
