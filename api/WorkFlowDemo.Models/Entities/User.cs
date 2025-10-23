using SqlSugar;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("Users")]
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}