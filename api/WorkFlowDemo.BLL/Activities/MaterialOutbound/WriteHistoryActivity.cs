using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    /// <summary>
    /// 写入履历活动
    /// </summary>
    [Activity("MaterialOutbound", "写入履历", "将出库记录写入履历表")]
    public class WriteHistoryActivity : CodeActivity<List<string>>
    {
        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        [Input(Description = "出库详细列表")]
        public Input<List<MaterialOutboundDetailDto>> Details { get; set; } = default!;

        [Input(Description = "操作人")]
        public Input<string> Operator { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var materialRepository = context.GetRequiredService<IMaterialRepository>();
            var logger = context.GetRequiredService<ILogger<WriteHistoryActivity>>();
            var batchNumber = BatchNumber.Get(context);
            var details = Details.Get(context);
            var operatorName = Operator.Get(context);

            logger.LogInformation("开始写入履历，批次号: {BatchNumber}", batchNumber);

            try
            {
                var historyIds = new List<string>();

                foreach (var detail in details)
                {
                    var history = new MaterialHistory
                    {
                        Id = Guid.NewGuid().ToString(),
                        BatchNumber = batchNumber,
                        MaterialCode = detail.MaterialCode,
                        Qty = detail.Qty,
                        OperationTime = DateTime.Now,
                        Operator = operatorName,
                        CreatimeTime = DateTime.Now
                    };

                    await materialRepository.InsertHistoryAsync(history);
                    historyIds.Add(history.Id);
                }

                logger.LogInformation("成功写入履历，批次号: {BatchNumber}, 记录数: {Count}",
                    batchNumber, historyIds.Count);

                context.Set(Result, historyIds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "写入履历失败，批次号: {BatchNumber}", batchNumber);
                throw;
            }
        }
    }
}