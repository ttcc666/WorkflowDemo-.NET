using Elsa.Workflows.Models;

namespace WorkFlowDemo.BLL.Services.Demo
{
    public interface IMaterialOutBoundWorkflowService
    {
        /// <summary>
        /// 启动物料出库工作流
        /// </summary>
        /// <param name="materialOutBatchNo"></param>
        /// <returns></returns>
        Task<RunWorkflowResult> StartMaterialOutBoundWorkflowAsync(string materialOutBatchNo);
    }
}