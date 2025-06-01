// This is a personal academic project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversityGradesSystem.Helpers
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null)
                return defaultValue;

            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}