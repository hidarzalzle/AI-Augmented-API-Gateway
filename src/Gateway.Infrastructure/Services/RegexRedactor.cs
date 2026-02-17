using System.Text.RegularExpressions;
using Gateway.Core.Abstractions;

namespace Gateway.Infrastructure.Services;

public sealed partial class RegexRedactor : IRedactor
{
    [GeneratedRegex("(?i)(password|token|secret)\\s*[:=]\\s*[^,\\s]+")]
    private static partial Regex SecretRegex();

    public string Redact(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return SecretRegex().Replace(input, "$1=***");
    }
}
