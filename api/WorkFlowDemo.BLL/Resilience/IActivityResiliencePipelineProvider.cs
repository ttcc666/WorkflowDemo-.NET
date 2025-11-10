using Polly;

namespace WorkFlowDemo.BLL.Resilience;

public interface IActivityResiliencePipelineProvider
{
    bool TryGetPipeline<TResult>(Type activityType, out ResiliencePipeline<TResult>? pipeline);
}
