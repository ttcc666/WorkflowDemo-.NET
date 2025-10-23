using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities
{
    /// <summary>
    /// 库存预留活动
    /// </summary>
    public class ReserveInventoryActivity : CodeActivity<bool>
    {
        [Input(Description = "产品ID")]
        public Input<string> ProductId { get; set; } = default!;

        [Input(Description = "数量")]
        public Input<int> Quantity { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<ReserveInventoryActivity>>();
            var productId = ProductId.Get(context);
            var quantity = Quantity.Get(context);

            logger.LogInformation("开始库存预留: ProductId={ProductId}, Quantity={Quantity}", productId, quantity);

            // 模拟库存预留逻辑
            await Task.Delay(100);
            var success = true; // 假设预留成功

            logger.LogInformation("库存预留完成: Success={Success}", success);
            context.Set(Result, success);
        }
    }
}