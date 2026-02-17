namespace Gateway.Core.Abstractions;

public interface IRedactor
{
    string Redact(string input);
}
