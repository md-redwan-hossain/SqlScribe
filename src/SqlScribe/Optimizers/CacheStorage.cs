using System.Collections.Concurrent;
using System.Collections.Generic;
using SqlScribe.Enums;

namespace SqlScribe.Optimizers;

internal static class CacheStorage
{
    internal static readonly ConcurrentDictionary<(string propertyName, SqlNamingConvention convention), string>
        NamingConventionCache = new();

    internal static readonly ConcurrentDictionary<ExpressionKey, IReadOnlyList<string>> PropertyNameCache
        = new();
}