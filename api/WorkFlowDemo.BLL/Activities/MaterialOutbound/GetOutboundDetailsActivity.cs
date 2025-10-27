using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.BLL.Activities.Common;
using WorkFlowDemo.DAL.Repositories;
using WorkFlowDemo.Models.Dtos;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    [Activity("MaterialOutbound", "获取出库详细", "根据批次号获取出库详细信息")]
    public class GetOutboundDetailsActivity : BaseActivity<List<MaterialOutboundDetailDto>>
    {
        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        protected override async ValueTask<List<MaterialOutboundDetailDto>> ExecuteActivityAsync(ActivityExecutionContext context, ILogger logger)
        {
            var batchNumber = BatchNumber.Get(context);
            var repository = context.GetRequiredService<IMaterialTemporaryScanRepository>();
            var records = await repository.GetByBatchNumberAsync(batchNumber);

            return records?.Select(s => new MaterialOutboundDetailDto
            {
                MaterialCode = s.MaterialCode,
                Qty = s.Qty
            }).ToList() ?? new List<MaterialOutboundDetailDto>();
        }
    }
}