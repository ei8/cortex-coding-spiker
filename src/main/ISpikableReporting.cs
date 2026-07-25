using System;

namespace ei8.Cortex.Coding.Spiker
{
    public interface ISpikableReporting : ISpikable
    {
        float ProcessingRatio { get; }

        event EventHandler<TriggeredEventArgs>? Triggered;

        event EventHandler<FiredEventArgs>? Fired;
    }
}
