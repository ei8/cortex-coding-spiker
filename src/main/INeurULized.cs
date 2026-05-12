using ei8.Cortex.Coding;

namespace ei8.Cortex.Coding.Spiker
{
    public interface INeurULized
    {
        Network? Network { get; }

        void Initialize(Network? network);
    }
}
