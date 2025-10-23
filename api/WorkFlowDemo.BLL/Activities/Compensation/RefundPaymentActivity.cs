using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.Compensation {
    /// <summary>
    /// 退款活动
    /// </summary>
    public class RefundPaymentActivity : CodeActivity
    {
        [Input(Description = "支付ID")]
        public Input<string> PaymentId { get; set; } = default!;

        [Input(Description = "退款金额")]
        public Input<decimal> Amount { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<RefundPaymentActivity>>();
            var paymentId = PaymentId.Get(context);
            var amount = Amount.Get(context);

            logger.LogWarning("执行退款: PaymentId={PaymentId}, Amount={Amount}", paymentId, amount);

            await Task.Delay(50);

            logger.LogInformation("退款已完成");
        }
    }
}