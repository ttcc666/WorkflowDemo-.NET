using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;
using WorkFlowDemo.DAL.Repositories;

namespace WorkFlowDemo.BLL.Activities.MaterialOutbound
{
    /// <summary>
    /// 删除扫描记录活动
    /// </summary>
    [Activity("MaterialOutbound", "删除扫描记录", "删除临时扫描记录")]
    public class DeleteScanRecordsActivity : CodeActivity<bool>
    {
        [Input(Description = "批次号")]
        public Input<string> BatchNumber { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var scanDal = context.GetRequiredService<MaterialTemporaryScanDal>();
            var logger = context.GetRequiredService<ILogger<DeleteScanRecordsActivity>>();
            var batchNumber = BatchNumber.Get(context);
            
            logger.LogInformation("开始删除扫描记录，批次号: {BatchNumber}", batchNumber);

            try
            {
                var success = await scanDal.DeleteByBatchNumberAsync(batchNumber);
                
                if (!success)
                {
                    logger.LogError("删除扫描记录失败，批次号: {BatchNumber}", batchNumber);
                    context.Set(Result, false);
                    return;
                }

                logger.LogInformation("成功删除扫描记录，批次号: {BatchNumber}", batchNumber);
                context.Set(Result, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "删除扫描记录失败，批次号: {BatchNumber}", batchNumber);
                throw;
            }
        }
    }
}