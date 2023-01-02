namespace SMSTerminal.Interfaces
{
    public interface IOutputParser
    {
        public Task<bool> ParseModemOutput(string modemOutput);
    }
}
