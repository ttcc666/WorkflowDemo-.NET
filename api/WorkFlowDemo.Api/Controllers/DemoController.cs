

using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Entities;
using Microsoft.AspNetCore.Mvc;
using WorkFlowDemo.BLL.Services.Demo;

namespace WorkFlowDemo.Api.Controllers
{
    public class DemoController : BaseController
    {
        private readonly IWorkflowService _workflowServices;
        private readonly IMaterialOutBoundWorkflowService _materialOutBoundWorkflowService;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;


        public DemoController(IWorkflowService workflowServices, IMaterialOutBoundWorkflowService materialOutBoundWorkflowService,
        IWorkflowInstanceStore workflowInstanceStore)
        {
            _workflowServices = workflowServices;
            _materialOutBoundWorkflowService = materialOutBoundWorkflowService;
            _workflowInstanceStore = workflowInstanceStore;
        }

        /// <summary>
        /// 启动物料出库工作流
        /// </summary>
        /// <param name="materialOutBatchNo"></param>
        /// <returns></returns>
        [HttpGet("StartMaterialOutBoundWorkflow")]
        public async Task<IActionResult> StartMaterialOutBoundWorkflow(string materialOutBatchNo)
        {
            var result = await _materialOutBoundWorkflowService.StartMaterialOutBoundWorkflowAsync(materialOutBatchNo);
            return Success(result.WorkflowState);
        }

        /// <summary>
        /// 通过 工作流ID 查询 工作流状态
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        [HttpGet("GetMaterialOutBoundWorkflowState")]
        public async Task<IActionResult> GetMaterialOutBoundWorkflowState(string workflowId)
        {
            var filter = new WorkflowInstanceFilter { Id = workflowId };
            var workflowInstance = await _workflowInstanceStore.FindAsync(filter);
            return workflowInstance == null ? Fail("工作流不存在", 404) : Success(workflowInstance);
        }
    }
}