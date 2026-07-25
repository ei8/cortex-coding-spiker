using ei8.Cortex.Coding.Mirrors;
using System;
using System.Collections.Generic;

namespace ei8.Cortex.Coding.Spiker
{
    public interface ISpikable : IneurUL
    {
        IDictionary<DateTime, FireInfo> FireHistory { get; }

        void Initialize(IEnumerable<MirrorConfig>? mirrorConfigs);

        TimeSpan RefractoryPeriod { get; set; }

        TimeSpan RelatedSpikesPeriod { get; set; }

        void Spike(params Neuron[] neurons);
    }
}
