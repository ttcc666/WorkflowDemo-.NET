using SqlSugar;
using WorkFlowDemo.DAL.Base;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.DAL.Repositories
{
    public class WorkflowBatchBindingRepository : BaseRepository<WorkflowBatchBinding>, IWorkflowBatchBindingRepository
    {
        public WorkflowBatchBindingRepository(ISqlSugarClient db) : base(db)
        {
        }
    }
}