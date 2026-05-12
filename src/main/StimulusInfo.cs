namespace ei8.Cortex.Coding.Spiker
{
    public class StimulusInfo(StimulusType type, object value)
    {
        public StimulusType Type { get; } = type;

        public object Value { get; } = value;
    }
}