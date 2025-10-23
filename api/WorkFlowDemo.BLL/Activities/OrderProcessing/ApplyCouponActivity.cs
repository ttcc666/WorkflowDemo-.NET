using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities.OrderProcessing {
    /// <summary>
    /// 优惠券核销活动
    /// </summary>
    public class ApplyCouponActivity : CodeActivity<decimal>
    {
        [Input(Description = "优惠券代码")]
        public Input<string?> CouponCode { get; set; } = default!;

        [Input(Description = "订单金额")]
        public Input<decimal> OrderAmount { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<ApplyCouponActivity>>();
            var couponCode = CouponCode.Get(context);
            var orderAmount = OrderAmount.Get(context);

            logger.LogInformation("开始优惠券核销: CouponCode={CouponCode}, OrderAmount={OrderAmount}", couponCode, orderAmount);

            await Task.Delay(100);
            var discountedAmount = string.IsNullOrEmpty(couponCode) ? orderAmount : orderAmount * 0.9m;

            logger.LogInformation("优惠券核销完成: DiscountedAmount={DiscountedAmount}", discountedAmount);
            context.Set(Result, discountedAmount);
        }
    }
}