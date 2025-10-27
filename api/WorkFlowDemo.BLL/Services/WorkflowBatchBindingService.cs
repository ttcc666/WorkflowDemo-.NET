using WorkFlowDemo.BLL.Base;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Services
{
    public class WorkflowBatchBindingService : BaseService<WorkflowBatchBinding>, IWorkflowBatchBindingService
    {
        public WorkflowBatchBindingService(IWorkflowBatchBindingRepository repository) : base(repository)
        {
        }
    }
}