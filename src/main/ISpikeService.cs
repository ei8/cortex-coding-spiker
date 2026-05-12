using System;
using System.Collections.Generic;

namespace ei8.Cortex.Coding.Spiker
{
    public interface ISpikeService
    {
        event EventHandler<TriggeredEventArgs>? Triggered;

        event EventHandler<FiredEventArgs>? Fired;

        void SetSpikeCount(int value);

        void Spike(IEnumerable<Neuron> targets, Network network, TimeSpan refractoryPeriod);
    }
}
