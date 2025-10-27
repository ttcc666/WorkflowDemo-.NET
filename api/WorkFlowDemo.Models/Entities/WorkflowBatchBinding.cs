using SqlSugar;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("WorkflowBatchBinding")]
    public class WorkflowBatchBinding
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }=Guid.NewGuid().ToString();
        
        public string BatchNo { get; set; } = string.Empty;
        
        public string WorkflowInstanceId { get; set; } = string.Empty;
    }
}