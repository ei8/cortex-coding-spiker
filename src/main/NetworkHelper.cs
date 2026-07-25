using ei8.Cortex.Coding.Mirrors;
using neurUL.Common.Domain.Model;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Linq;

namespace ei8.Cortex.Coding.Spiker
{
    // TODO: Use C# 14+ Extension Members and attach to Neuron and Terminal classes
    public static class NetworkHelper
    {
        public static bool TryCreateNeuron(
            [NotNullWhen(true)] out Neuron? result,
            [CallerArgumentExpression(nameof(result))] string parameterExpression = ""
        )
        {
            bool bResult = false;
            result = null;
            if (VariableInfo.TryParse(parameterExpression, out var variable))
            {
                result = NetworkHelper.CreateNeuron(variable.Inputs.Single());
                bResult = true;
            }

            return bResult;
        }

        public static Neuron CreateNeuron(string? tag = null) =>
            Neuron.CreateTransient(Guid.NewGuid(), tag, null, null);

        public static Network CreateInterneuronNetwork(params Neuron[] postsynapticNeurons) =>
            NetworkHelper.CreateInterneuronNetwork(null, postsynapticNeurons);

        public static Network CreateInterneuronNetwork(string? interneuronTag = null, params Neuron[] postsynapticNeurons)
        {
            var network = new Network();
            Neuron neuron = NetworkHelper.CreateNeuron(interneuronTag);
            network.AddReplace(neuron);

            foreach (var post in postsynapticNeurons)
                network.AddReplace(NetworkHelper.CreateTerminal(neuron, post));

            return network;
        }

        public static Network LinkInputNeuronsToInterneuron(Neuron interneuron, params Neuron[] inputNeurons)
        {
            var network = new Network();
            foreach (Neuron input in inputNeurons)
                network.AddReplace(NetworkHelper.CreateTerminal(input, interneuron, NeurotransmitterEffect.Excite, 1f / inputNeurons.Length));
            return network;
        }

        public static Network CreateInputNeuronNetwork(MirrorConfig mirrorConfig, float strengthToInterneurons, params Network[] interneurons) =>
            NetworkHelper.CreateInputNeuronNetwork(mirrorConfig, strengthToInterneurons, [.. interneurons.Select(i => i.GetInterneuron())]);
        
        public static Network CreateInputNeuronNetwork(MirrorConfig mirrorConfig, float strengthToInterneurons, params Neuron[] interneurons)
        {
            AssertionConcern.AssertArgumentNotNull(mirrorConfig, nameof(mirrorConfig));

            var result = new Network();
            var inputNeuron = NetworkHelper.CreateNeuron(mirrorConfig);
            result.AddReplace(inputNeuron);

            foreach (var interneuron in interneurons)
                result.AddReplace(NetworkHelper.CreateTerminal(inputNeuron, interneuron, NeurotransmitterEffect.Excite, strengthToInterneurons));

            return result;
        }

        public static Neuron CreateNeuron(
            MirrorConfig mirrorConfig
        )
        {
            AssertionConcern.AssertArgumentNotNull(mirrorConfig, nameof(mirrorConfig));

            return Neuron.CreateTransient(Guid.NewGuid(), string.Join(',', mirrorConfig.Keys), mirrorConfig.Url, null);
        }

        public static Neuron CreateNeuron() =>
            Neuron.CreateTransient(Guid.NewGuid(), null, null, null);

        public static Terminal CreateTerminal(
            Neuron presynapticNeuron,
            Neuron postsynapticNeuron
        ) => NetworkHelper.CreateTerminal(presynapticNeuron, postsynapticNeuron, NeurotransmitterEffect.Excite, 1f);

        public static Terminal CreateTerminal(
            Neuron presynapticNeuron,
            Neuron postsynapticNeuron,
            NeurotransmitterEffect effect,
            float strength
        ) => new(Guid.NewGuid(), presynapticNeuron.Id, postsynapticNeuron.Id, effect, strength);
    }
}
