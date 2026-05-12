using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ei8.Cortex.Coding.Spiker
{
    public delegate bool ParameterConverter(FireInfo fireInfo, [NotNullWhen(true)] out object? result);

    public class ResponseParser(
        Predicate<FireInfo> evaluator, 
        Guid actionNeuronId,
        IEnumerable<ParameterConverter> parameterConverters
    )
    {
        public Predicate<FireInfo> Evaluator { get; } = evaluator;

        public Guid ActionNeuronId { get; } = actionNeuronId;

        public IEnumerable<ParameterConverter> ParameterConverters { get; } = parameterConverters;
    }
}
