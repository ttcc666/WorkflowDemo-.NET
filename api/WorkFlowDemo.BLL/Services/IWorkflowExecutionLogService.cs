using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public interface IWorkflowExecutionLogService : IBaseService<WorkflowExecutionLog>
    {
        Task<bool> LogAsync(WorkflowExecutionLog log);
        Task<List<WorkflowExecutionLog>> GetByWorkflowInstanceIdAsync(string workflowInstanceId);
        Task<List<WorkflowExecutionLog>> GetByBatchNumberAsync(string batchNumber);
        Task<List<WorkflowExecutionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<WorkflowExecutionLog>> GetFailedLogsAsync(int pageIndex = 1, int pageSize = 50);
    }
}