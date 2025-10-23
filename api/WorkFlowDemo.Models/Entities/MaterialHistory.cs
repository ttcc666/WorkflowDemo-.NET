using SqlSugar;
using System;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("MaterialHistory")]
    public class MaterialHistory
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


        [SugarColumn(ColumnName = "CreatimeTime")]
        public DateTime CreatimeTime { get; set; }
    }
}
