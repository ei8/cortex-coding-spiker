using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ei8.Cortex.Coding.Spiker
{
    public static class ExtensionMethods
    {
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
    }
}
