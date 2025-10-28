using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public interface IWorkflowExecutionLogRepository : IBaseRepository<WorkflowExecutionLog>
    {
        Task<List<WorkflowExecutionLog>> GetByWorkflowInstanceIdAsync(string workflowInstanceId);
        Task<List<WorkflowExecutionLog>> GetByBatchNumberAsync(string batchNumber);
        Task<List<WorkflowExecutionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<WorkflowExecutionLog>> GetFailedLogsAsync(int pageIndex, int pageSize);
    }
}