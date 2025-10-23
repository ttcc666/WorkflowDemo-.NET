using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.OrderProcessing {
    /// <summary>
    /// 创建发货单活动
    /// </summary>
    public class CreateShipmentActivity : CodeActivity<string>
    {
        [Input(Description = "产品ID")]
        public Input<string> ProductId { get; set; } = default!;

        [Input(Description = "数量")]
        public Input<int> Quantity { get; set; } = default!;

        [Input(Description = "用户ID")]
        public Input<string> UserId { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<CreateShipmentActivity>>();
            var productId = ProductId.Get(context);
            var quantity = Quantity.Get(context);
            var userId = UserId.Get(context);

            logger.LogInformation("开始创建发货单: ProductId={ProductId}, Quantity={Quantity}, UserId={UserId}", productId, quantity, userId);

            await Task.Delay(100);
            var shipmentId = $"SHIP-{Guid.NewGuid():N}";

            logger.LogInformation("发货单创建完成: ShipmentId={ShipmentId}", shipmentId);
            context.Set(Result, shipmentId);
        }
    }
}