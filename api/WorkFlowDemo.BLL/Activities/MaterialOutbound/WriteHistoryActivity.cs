using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    [Activity("MaterialOutbound", "写入履历", "将出库记录写入履历表")]
    public class WriteHistoryActivity : BaseActivity<List<string>>
    {
        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        [Input(Description = "出库详细列表")]
        public Input<List<MaterialOutboundDetailDto>> Details { get; set; } = default!;

        [Input(Description = "操作人")]
        public Input<string> Operator { get; set; } = default!;

        protected override async ValueTask<List<string>> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger)
        {
            var batchNumber = BatchNumber.Get(context);
            var details = Details.Get(context);
            var operatorName = Operator.Get(context);
            var repository = context.GetRequiredService<IMaterialRepository>();
            var historyIds = new List<string>();
            var now = DateTime.Now;

            foreach (var detail in details)
            {
                var history = new MaterialHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    BatchNumber = batchNumber,
                    MaterialCode = detail.MaterialCode,
                    Qty = detail.Qty,
                    OperationTime = now,
                    Operator = operatorName,
                    CreatimeTime = now
                };

                await repository.InsertHistoryAsync(history);
                historyIds.Add(history.Id);
            }

            return historyIds;
        }
    }
}