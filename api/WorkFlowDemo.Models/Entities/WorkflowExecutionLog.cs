using SqlSugar;

namespace WorkFlowDemo.Models.Entities
{
    [SugarTable("WorkflowExecutionLog")]
    [SugarIndex("index_workflowlog_instanceid", nameof(WorkflowInstanceId), OrderByType.Asc)]
    [SugarIndex("index_workflowlog_batchnumber", nameof(BatchNumber), OrderByType.Asc)]
    [SugarIndex("index_workflowlog_status", nameof(ExecutionStatus), OrderByType.Asc)]
    [SugarIndex("index_workflowlog_createtime", nameof(CreatedTime), OrderByType.Desc)]
    [SugarIndex("index_workflowlog_batch_time", nameof(BatchNumber), OrderByType.Asc, nameof(CreatedTime), OrderByType.Desc)]
    public class WorkflowExecutionLog
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [SugarColumn(IsNullable = false, ColumnDescription = "工作流实例ID")]
        public string WorkflowInstanceId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, Length = 200, ColumnDescription = "工作流定义名称")]
        public string WorkflowDefinitionName { get; set; } = string.Empty;

        [SugarColumn(IsNullable = true, Length = 100, ColumnDescription = "活动节点ID")]
        public string? ActivityId { get; set; }

        [SugarColumn(IsNullable = true, Length = 200, ColumnDescription = "活动节点名称")]
        public string? ActivityName { get; set; }

        [SugarColumn(IsNullable = false, Length = 100, ColumnDescription = "步骤名称")]
        public string StepName { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, ColumnDescription = "节点序号")]
        public int StepOrder { get; set; }

        [SugarColumn(IsNullable = false, Length = 500, ColumnDescription = "状态消息")]
        public string StatusMessage { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, Length = 50, ColumnDescription = "执行状态")]
        public string ExecutionStatus { get; set; } = "Running";

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)", ColumnDescription = "错误信息")]
        public string? ErrorMessage { get; set; }

        [SugarColumn(IsNullable = true, Length = 100, ColumnDescription = "业务批次号")]
        public string? BatchNumber { get; set; }

        [SugarColumn(IsNullable = true, Length = 100, ColumnDescription = "操作人")]
        public string? Operator { get; set; }

        [SugarColumn(IsNullable = true, ColumnDescription = "执行耗时(毫秒)")]
        public long? ExecutionDuration { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)", ColumnDescription = "输入数据")]
        public string? InputData { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)", ColumnDescription = "输出数据")]
        public string? OutputData { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)", ColumnDescription = "扩展数据")]
        public string? ExtendedData { get; set; }

        [SugarColumn(IsNullable = false, ColumnDescription = "创建时间")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}