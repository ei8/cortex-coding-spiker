using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ei8.Cortex.Coding.Spiker
{
    public class SpikeInfo()
    {
        public IDictionary<DateTime, TriggerInfo> Triggers { get; } = new ConcurrentDictionary<DateTime, TriggerInfo>();
        public FireInfo? LastFire { get; set; }
    }
}
