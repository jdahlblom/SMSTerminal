namespace SMSTerminal.Events;



public class ATCommandEventArgs : EventArgs
{
    public string ModemId { get; init; }
    public string ATCommand { get; init; }
    public string Message { get; init; }
    public string ErrorMessage { get; init; }
    public ModemEventType EventType { get; init; }
    public ModemResultEnum ResultStatus { get; init; }
}