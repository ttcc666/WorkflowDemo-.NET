using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.DAL.Repositories;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    /// <summary>
    /// 回滚履历活动
    /// </summary>
    [Activity("MaterialOutbound", "回滚履历", "删除已写入的履历记录")]
    public class RollbackHistoryActivity : CodeActivity<bool>
    {
        [Input(Description = "履历ID列表")]
        public Input<List<string>> HistoryIds { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var materialDal = context.GetRequiredService<MaterialDal>();
            var logger = context.GetRequiredService<ILogger<RollbackHistoryActivity>>();
            var historyIds = HistoryIds.Get(context);
            
            logger.LogInformation("开始回滚履历，记录数: {Count}", historyIds?.Count ?? 0);

            try
            {
                if (historyIds == null || !historyIds.Any())
                {
                    logger.LogWarning("履历ID列表为空，无需回滚");
                    context.Set(Result, true);
                    return;
                }

                var success = await materialDal.DeleteHistoryByIdsAsync(historyIds);
                
                if (!success)
                {
                    logger.LogError("回滚履历失败");
                    context.Set(Result, false);
                    return;
                }

                logger.LogInformation("成功回滚履历，记录数: {Count}", historyIds.Count);
                context.Set(Result, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "回滚履历失败");
                throw;
            }
        }
    }
}