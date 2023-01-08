namespace SMSTerminal.Interfaces
{
    public interface IOutputParser
    {
        public Task<string> ParseModemOutput(string modemOutput);
    }
}
