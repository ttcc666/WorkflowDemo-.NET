using Microsoft.AspNetCore.Mvc;
using WorkFlowDemo.BLL.Services;
using WorkFlowDemo.Models.Common;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowLogController : BaseController
    {
        private readonly IWorkflowExecutionLogService _logService;

        public WorkflowLogController(IWorkflowExecutionLogService logService)
        {
            _logService = logService;
        }

        [HttpGet("instance/{workflowInstanceId}")]
        public async Task<ApiResponse<List<WorkflowExecutionLog>>> GetByWorkflowInstanceId(string workflowInstanceId)
        {
            var logs = await _logService.GetByWorkflowInstanceIdAsync(workflowInstanceId);
            return ApiResponse<List<WorkflowExecutionLog>>.Success(logs);
        }

        [HttpGet("batch/{batchNumber}")]
        public async Task<ApiResponse<List<WorkflowExecutionLog>>> GetByBatchNumber(string batchNumber)
        {
            var logs = await _logService.GetByBatchNumberAsync(batchNumber);
            return ApiResponse<List<WorkflowExecutionLog>>.Success(logs);
        }

        [HttpGet("failed")]
        public async Task<ApiResponse<List<WorkflowExecutionLog>>> GetFailedLogs([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
        {
            var logs = await _logService.GetFailedLogsAsync(pageIndex, pageSize);
            return ApiResponse<List<WorkflowExecutionLog>>.Success(logs);
        }

        [HttpGet("range")]
        public async Task<ApiResponse<List<WorkflowExecutionLog>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await _logService.GetByDateRangeAsync(startDate, endDate);
            return ApiResponse<List<WorkflowExecutionLog>>.Success(logs);
        }
    }
}