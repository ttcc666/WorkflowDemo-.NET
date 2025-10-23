using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.OrderProcessing {
    /// <summary>
    /// 支付扣款活动
    /// </summary>
    public class ProcessPaymentActivity : CodeActivity<string>
    {
        [Input(Description = "订单金额")]
        public Input<decimal> OrderAmount { get; set; } = default!;

        [Input(Description = "用户ID")]
        public Input<string> UserId { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<ProcessPaymentActivity>>();
            var orderAmount = OrderAmount.Get(context);
            var userId = UserId.Get(context);

            logger.LogInformation("开始支付扣款: UserId={UserId}, OrderAmount={OrderAmount}", userId, orderAmount);

            await Task.Delay(100);
            var paymentId = $"PAY-{Guid.NewGuid():N}";

            logger.LogInformation("支付扣款完成: PaymentId={PaymentId}", paymentId);
            context.Set(Result, paymentId);
        }
    }
}