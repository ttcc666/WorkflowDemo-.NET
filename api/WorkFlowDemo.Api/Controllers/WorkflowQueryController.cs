using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.Api.Controllers
{
    public class WorkflowQueryController : BaseController
    {
        private readonly ISqlSugarClient _db;

        public WorkflowQueryController(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 查询所有物料出库历史记录
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllWorkflows")]
        public async Task<IActionResult> GetAllWorkflows()
        {
            var histories = await _db.Queryable<MaterialHistory>()
                .OrderByDescending(h => h.CreatimeTime)
                .ToListAsync();
            
            var result = histories.GroupBy(h => h.BatchNumber)
                .Select(g => new
                {
                    batchNumber = g.Key,
                    operator_ = g.First().Operator,
                    creatimeTime = g.First().CreatimeTime,
                    itemCount = g.Count(),
                    totalQty = g.Sum(h => h.Qty)
                });
            
            return Success(result);
        }

        /// <summary>
        /// 根据批号查询物料出库详情
        /// </summary>
        /// <param name="batchNumber"></param>
        /// <returns></returns>
        [HttpGet("GetWorkflowByBatch")]
        public async Task<IActionResult> GetWorkflowByBatch([FromQuery] string batchNumber)
        {
            if (string.IsNullOrEmpty(batchNumber))
            {
                return Fail("批号不能为空", 400);
            }

            var histories = await _db.Queryable<MaterialHistory>()
                .Where(h => h.BatchNumber == batchNumber)
                .OrderBy(h => h.CreatimeTime)
                .ToListAsync();
            
            if (!histories.Any())
            {
                return Fail("未找到相关记录", 404);
            }

            var result = new
            {
                batchNumber = batchNumber,
                operator_ = histories.First().Operator,
                creatimeTime = histories.First().CreatimeTime,
                items = histories.Select(h => new
                {
                    id = h.Id,
                    materialCode = h.MaterialCode,
                    qty = h.Qty,
                    operationTime = h.OperationTime
                })
            };

            return Success(result);
        }
    }
}