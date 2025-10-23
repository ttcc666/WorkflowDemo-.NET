using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.Compensation {
    /// <summary>
    /// 回滚库存预留活动
    /// </summary>
    public class RollbackInventoryActivity : CodeActivity
    {
        [Input(Description = "产品ID")]
        public Input<string> ProductId { get; set; } = default!;

        [Input(Description = "数量")]
        public Input<int> Quantity { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<RollbackInventoryActivity>>();
            var productId = ProductId.Get(context);
            var quantity = Quantity.Get(context);

            logger.LogWarning("回滚库存预留: ProductId={ProductId}, Quantity={Quantity}", productId, quantity);

            await Task.Delay(50);

            logger.LogInformation("库存预留已回滚");
        }
    }
}