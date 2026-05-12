namespace ei8.Cortex.Coding.Spiker
{
    public interface ISpikableReporting : ISpikable
    {
        float ProcessingRatio { get; }
    }
}
