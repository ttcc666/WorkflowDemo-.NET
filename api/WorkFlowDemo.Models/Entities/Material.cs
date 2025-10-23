using SqlSugar;
using System;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("Material")]
    public class Material
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; } = string.Empty;
        [SugarColumn(ColumnName = "MaterialCode")]
        public string MaterialCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
