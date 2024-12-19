using System.Text.RegularExpressions;

namespace SqlScribe.HttpApiExample.Utils;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is string strValue)
        {
            return Regex.Replace(strValue,
                "([a-z])([A-Z])",
                "$1-$2",
                RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(100)).ToLowerInvariant();
        }

        return null;
    }
}