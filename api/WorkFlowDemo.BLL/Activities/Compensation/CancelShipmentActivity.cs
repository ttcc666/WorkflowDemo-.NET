using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.Compensation {
    /// <summary>
    /// 取消发货单活动
    /// </summary>
    public class CancelShipmentActivity : CodeActivity
    {
        [Input(Description = "发货单ID")]
        public Input<string> ShipmentId { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<CancelShipmentActivity>>();
            var shipmentId = ShipmentId.Get(context);

            logger.LogWarning("取消发货单: ShipmentId={ShipmentId}", shipmentId);

            await Task.Delay(50);

            logger.LogInformation("发货单已取消");
        }
    }
}