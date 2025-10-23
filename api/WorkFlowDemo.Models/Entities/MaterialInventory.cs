

using SqlSugar;
using System;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("MaterialInventory")]
    public class MaterialInventory
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; } = string.Empty;
        [SugarColumn(ColumnName = "MaterialCode")]
        public string MaterialCode { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
