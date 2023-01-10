namespace SMSTerminal.Interfaces;

/// <summary>
/// Parses modem output to find end of message markers and then
/// separates that part to be processed further.
/// </summary>
public interface IOutputParser
{
    public Task<string> ParseModemOutput(string modemOutput);
}