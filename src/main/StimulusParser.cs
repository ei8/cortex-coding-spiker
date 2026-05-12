using System;

namespace ei8.Cortex.Coding.Spiker
{
    public class StimulusParser(StimulusType type, Predicate<object> evaluator, Func<object, Neuron> neuronConverter)
    {
        public StimulusType Type { get; } = type;

        public Predicate<object> Evaluator { get; } = evaluator;

        public Func<object, Neuron> NeuronConverter { get; } = neuronConverter;
    } 
}
