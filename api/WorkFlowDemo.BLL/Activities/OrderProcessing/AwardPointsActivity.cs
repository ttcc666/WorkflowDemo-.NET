using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace WorkFlowDemo.BLL.Activities
{
    /// <summary>
    /// 积分发放活动
    /// </summary>
    public class AwardPointsActivity : CodeActivity<int>
    {
        [Input(Description = "订单金额")]
        public Input<decimal> OrderAmount { get; set; } = default!;

        [Input(Description = "用户ID")]
        public Input<string> UserId { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var logger = context.GetRequiredService<ILogger<AwardPointsActivity>>();
            var orderAmount = OrderAmount.Get(context);
            var userId = UserId.Get(context);

            logger.LogInformation("开始积分发放: UserId={UserId}, OrderAmount={OrderAmount}", userId, orderAmount);

            await Task.Delay(100);
            var points = (int)(orderAmount * 0.1m); // 10%积分返还

            logger.LogInformation("积分发放完成: Points={Points}", points);
            context.Set(Result, points);
        }
    }
}