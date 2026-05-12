using ei8.Cortex.Coding.Mirrors;
using ei8.Cortex.Coding.Model.Reflection;
using neurUL.Common.Domain.Model;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ei8.Cortex.Coding.Spiker
{
    public static class ExtensionMethods
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool TryParseSensoryNeurons(
            this ISpikable spikable,
            IEnumerable<StimulusParser> parsers,
            [NotNullWhen(true)] out IEnumerable<ParseSensorResult>? result,
            params StimulusInfo[] stimuli
        )
        {
            var bResult = false;
            result = null;
            var tempList = new List<ParseSensorResult>();

            foreach (var stimulus in stimuli)
            {
                StimulusParser? parser = null;
                if (
                    (
                        parser = parsers
                            .Where(p => p.Type == stimulus.Type)
                            .SingleOrDefault(p => p.Evaluator(stimulus.Value))
                    ) != null
                )
                    tempList.Add(new(stimulus.Value, parser.NeuronConverter(stimulus.Value)));
            }

            if (tempList.Count() == stimuli.Length)
            {
                bResult = true;
                result = tempList;
            }

            return bResult;
        }

        private static readonly object parseLock = new object();

        /// <summary>
        /// This might be a temporary approach. 
        /// Ideally, the fired neurons for a method and its parameters
        /// should be retrieved via mirrors if necessary, deneurULized, cached and invoked accordingly. 
        /// eg. Rotate Method (granny), Clockwise Direction Parameter (granny), 22.5 Float Degrees Parameter (granny)
        /// Using Method (class), MethodParameter (class)
        /// </summary>
        /// <param name="spikable"></param>
        /// <param name="currentFire"></param>
        /// <param name="responseParsers"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseMotorNeurons(
            this ISpikable spikable,
            FireInfo currentFire,
            IEnumerable<ResponseParser> responseParsers,
            [NotNullWhen(true)] out IEnumerable<ParseMotorResult>? result
        )
        {
            bool bResult = false;
            result = null;
            var tempResults = new List<ParseMotorResult>();

            lock (ExtensionMethods.parseLock)
            {
                ConcurrentDictionary<DateTime, FireInfo> fireHistory = (ConcurrentDictionary<DateTime, FireInfo>)spikable.FireHistory;
                fireHistory.Clean(currentFire.Timestamp.Subtract(spikable.RelatedSpikesPeriod));
                fireHistory.TryAdd(currentFire.Timestamp, currentFire);

                // if last fired is one of the anticipated neurons
                // TODO: anticipated neurons can include instantiates grannies (eg. instantiates^methodParameter) to optimize recognition,
                // ie. no need to recognize all possible values
                foreach (var rp in responseParsers)
                    if (currentFire.Target.Id == rp.ActionNeuronId || rp.Evaluator(currentFire))
                    {
                        if (fireHistory.Count() >= ExtensionMethods.GetExpectedResultCount(rp))
                        {
                            var matchedActionNeurons = fireHistory.Select(fh => fh.Value).Where(n => n.Target.Id == rp.ActionNeuronId);

                            if (matchedActionNeurons.Count() > 1)
                                ExtensionMethods.logger.Warn(
                                    new LogMessageGenerator(
                                        () =>
                                            $"{matchedActionNeurons.First().Target.ToReadableString()} fired more than once within related spikes period ({spikable.RelatedSpikesPeriod.TotalMilliseconds} ms): " +
                                            $"{string.Join(", ", matchedActionNeurons.Select(man => man.Timestamp.TimeOfDay.TotalMilliseconds))}"
                                    )
                                );

                            var actionFireInfo = matchedActionNeurons.FirstOrDefault();
                            // and specified method was fired within relative spikes period
                            if (actionFireInfo != null)
                            {
                                tempResults.Add(new(actionFireInfo.Target, actionFireInfo, actionFireInfo.Target.Tag));
                                foreach (var pc in rp.ParameterConverters)
                                {
                                    foreach (var fi in fireHistory.Values)
                                    {
                                        if (pc.Invoke(fi, out object? objectResult))
                                        {
                                            tempResults.Add(new(fi.Target, fi, objectResult));

                                            if (tempResults.Count() == ExtensionMethods.GetExpectedResultCount(rp))
                                            {
                                                fireHistory.Clear();
                                                bResult = true;
                                                result = tempResults;
                                            }
                                            break;
                                        }
                                    }

                                    if (bResult)
                                        break;
                                }
                            }
                        }
                    }
            }

            return bResult;
        }

        private static int GetExpectedResultCount(ResponseParser responseParser) =>
            // if number of related fires equals eg. 2 parameters + 1 method
            responseParser.ParameterConverters.Count() + 1;

        public static bool TryGetFiredParameter<T>(this FireInfo fireInfo, IDictionary<Guid, T> paramValueMap, out object? parameter)
        {
            parameter = null;

            if (paramValueMap.ContainsKey(fireInfo.Target.Id))
                parameter = paramValueMap[fireInfo.Target.Id];

            return parameter != null;
        }

        public static void Clean<T>(this ConcurrentDictionary<DateTime, T> concurrentDictionary, DateTime maxTimestamp)
        {
            foreach (var nfi in concurrentDictionary)
                if (nfi.Key < maxTimestamp)
                    concurrentDictionary.Remove(nfi.Key, out _);
        }

        public static Neuron ValidateGet(this Network network, Guid id)
        {
            if (network.TryGetById(id, out Neuron neuron))
                return neuron;
            else
                throw new ArgumentException($"Neuron with specified Id '{id}' was not found.");
        }

        public static string ToReadableString(this Neuron neuron)
        {
            return $"{neuron.Id}:'{neuron.Tag}'";
        }

        public static bool TryGetAdd<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> valueCreator,
            [NotNullWhen(true)] out TValue? result
        )
            where TKey : notnull
        {
            var bResult = false;

            if (!dictionary.ContainsKey(key))
                dictionary.TryAdd(key, valueCreator(key));

            if (dictionary.TryGetValue(key, out TValue? getResult))
            {
                bResult = true;
                result = getResult;
            }
            else
                result = default;

            return bResult;
        }

        public static Neuron CreateInputNeuron(this Network network, MirrorConfig mirrorConfig, float strengthToInterneurons, params Neuron[] interneurons)
        {
            AssertionConcern.AssertArgumentNotNull(mirrorConfig, nameof(mirrorConfig));

            var result = network.CreateNeuron(mirrorConfig);

            foreach (var interneuron in interneurons)
                network.CreateTerminal(result, interneuron, NeurotransmitterEffect.Excite, strengthToInterneurons);

            return result;
        }

        public static Neuron CreateRotationInterneuron(this Network network, Neuron rotateNeuron, Neuron directionNeuron, Neuron degreesNeuron)
        {
            var result = network.CreateNeuron();

            network.CreateTerminal(result, rotateNeuron);
            network.CreateTerminal(result, directionNeuron);
            network.CreateTerminal(result, degreesNeuron);

            return result;
        }

        public static Neuron CreateNeuron(
            this Network network,
            MirrorConfig mirrorConfig
        )
        {
            AssertionConcern.AssertArgumentNotNull(mirrorConfig, nameof(mirrorConfig));

            var result = Neuron.CreateTransient(Guid.NewGuid(), string.Join(',', mirrorConfig.Keys), mirrorConfig.Url, null);
            network.AddReplace(result);
            return result;
        }

        public static Neuron CreateNeuron(
            this Network network
        )
        {
            var result = Neuron.CreateTransient(Guid.NewGuid(), null, null, null);
            network.AddReplace(result);
            return result;
        }

        public static Terminal CreateTerminal(
            this Network network,
            Neuron presynapticNeuron,
            Neuron postsynapticNeuron
        ) => network.CreateTerminal(presynapticNeuron, postsynapticNeuron, NeurotransmitterEffect.Excite, 1f);

        public static Terminal CreateTerminal(
            this Network network,
            Neuron presynapticNeuron,
            Neuron postsynapticNeuron,
            NeurotransmitterEffect effect,
            float strength
        )
        {
            var result = new Terminal(Guid.NewGuid(), presynapticNeuron.Id, postsynapticNeuron.Id, effect, strength);
            network.AddReplace(result);
            return result;
        }

        public static IDictionary<Guid, T> ConvertToNeuronValueMap<T>(this IEnumerable<T> values, IEnumerable<MirrorConfig> mirrorConfigs, Network network) where T : Enum
        {
            var result = new Dictionary<Guid, T>();

            foreach (var value in values)
            {
                Neuron? neuron = null;
                if (
                    !mirrorConfigs.TryGetMirrorNeuron(
                        value.ToKeyString(),
                        network,
                        out neuron
                    )
                )
                    throw new InvalidOperationException($"Failed retrieving NeuronValueMap for {value.ToKeyString()}");

                result.Add(neuron.Id, value);
            }

            return result;
        }
    }
}
