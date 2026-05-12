using System;
using System.Collections.Generic;

namespace ei8.Cortex.Coding.Spiker
{
    public class TriggeredEventArgs
    (
        Neuron target,
        ChargeInfo charge, 
        IEnumerable<FireInfo> reflexArc
    ) : EventArgs
    {
        public Neuron Target { get; } = target;
        public ChargeInfo Charge { get; } = charge;
        public IEnumerable<FireInfo> ReflexArc { get; } = reflexArc;
    }
}
