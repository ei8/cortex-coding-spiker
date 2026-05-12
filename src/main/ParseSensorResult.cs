using ei8.Cortex.Coding;

namespace ei8.Cortex.Coding.Spiker
{
    public class ParseSensorResult(object @object, Neuron value)
    {
        public object Object { get; } = @object;

        public Neuron Value { get; } = value;
    }
}
