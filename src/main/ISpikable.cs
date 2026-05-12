using ei8.Cortex.Coding.Mirrors;
using System;
using System.Collections.Generic;

namespace ei8.Cortex.Coding.Spiker
{
    public interface ISpikable : INeurULized
    {
        IDictionary<DateTime, FireInfo> FireHistory { get; }

        void Initialize(Network? network, IEnumerable<MirrorConfig>? mirrorConfigs);

        TimeSpan RefractoryPeriod { get; set; }

        TimeSpan RelatedSpikesPeriod { get; set; }
    }
}
