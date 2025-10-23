using SqlSugar;
using System;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("Material")]
    public class Material
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }
        [SugarColumn(ColumnName = "MaterialCode")]
        public string MaterialCode { get; set; }
        public string Name { get; set; }
    }
}
