using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public class WorkflowExecutionLogService : BaseService<WorkflowExecutionLog>, IWorkflowExecutionLogService
    {
        private readonly IWorkflowExecutionLogRepository _repository;

        public WorkflowExecutionLogService(IWorkflowExecutionLogRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<bool> LogAsync(WorkflowExecutionLog log)
        {
            await _repository.AddAsync(log);
            return true;
        }

        public async Task<List<WorkflowExecutionLog>> GetByWorkflowInstanceIdAsync(string workflowInstanceId)
        {
            return await _repository.GetByWorkflowInstanceIdAsync(workflowInstanceId);
        }

        public async Task<List<WorkflowExecutionLog>> GetByBatchNumberAsync(string batchNumber)
        {
            return await _repository.GetByBatchNumberAsync(batchNumber);
        }

        public async Task<List<WorkflowExecutionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _repository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<List<WorkflowExecutionLog>> GetFailedLogsAsync(int pageIndex = 1, int pageSize = 50)
        {
            return await _repository.GetFailedLogsAsync(pageIndex, pageSize);
        }
    }
}