namespace SMSTerminal.General
{
    public sealed class ModemErrorMessage
    {
        public ModemErrorMessage(ErrorType errorType, int code, string message)
        {
            ErrorType = errorType;
            Number = code;
            Message = message;
        }
        public int Number { get; }

        public string Message { get; }

        public ErrorType ErrorType { get; }
    }
}
