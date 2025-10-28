using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class WorkflowExecutionLogRepository : BaseRepository<WorkflowExecutionLog>, IWorkflowExecutionLogRepository
    {
        public WorkflowExecutionLogRepository(ISqlSugarClient db) : base(db)
        {
        }

        public async Task<List<WorkflowExecutionLog>> GetByWorkflowInstanceIdAsync(string workflowInstanceId)
        {
            return await _db.Queryable<WorkflowExecutionLog>()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId)
                .OrderBy(x => x.StepOrder)
                .ToListAsync();
        }

        public async Task<List<WorkflowExecutionLog>> GetByBatchNumberAsync(string batchNumber)
        {
            return await _db.Queryable<WorkflowExecutionLog>()
                .Where(x => x.BatchNumber == batchNumber)
                .OrderBy(x => x.StepOrder)
                .ToListAsync();
        }

        public async Task<List<WorkflowExecutionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.Queryable<WorkflowExecutionLog>()
                .Where(x => x.CreatedTime >= startDate && x.CreatedTime <= endDate)
                .OrderBy(x => x.CreatedTime, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<List<WorkflowExecutionLog>> GetFailedLogsAsync(int pageIndex, int pageSize)
        {
            return await _db.Queryable<WorkflowExecutionLog>()
                .Where(x => x.ExecutionStatus == "Failed")
                .OrderBy(x => x.CreatedTime, OrderByType.Desc)
                .ToPageListAsync(pageIndex, pageSize);
        }
    }
}