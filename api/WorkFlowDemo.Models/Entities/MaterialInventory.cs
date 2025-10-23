

using SqlSugar;
using System;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("MaterialInventory")]
    public class MaterialInventory
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }
        [SugarColumn(ColumnName = "MaterialCode")]
        public string MaterialCode { get; set; }
        public decimal Qty { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
