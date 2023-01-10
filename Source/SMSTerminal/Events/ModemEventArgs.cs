namespace SMSTerminal.Events;

public enum ModemEventType
{
    SMSSent,
    SMSReceived,
    SMSDeleted,
    SMSArrived,
    ModemMessage, // Unsolicited messages
    Stats,
    Reply,
    Data,
    Status,
    SendStatus,
    ReceiveStatus,
    PIN,
    WriteData,
    ReceiveData,
    NetworkStatus,
    NoNewSMSWaiting,
    ModemConfigurationStatus,
    ModemComms
}

public enum ModemResultEnum
{
    None,
    Ok,
    Error,
    CMEError,
    CMSError,
    Critical,
    IOError,
    TimeOutError,
    ParseFail,
    UnknownModemData
}

public class ModemEventArgs : EventArgs
{
    public string ModemId { get; init; }
    public string Message { get; init; }
    public ModemEventType EventType { get; init; }
    public string Id { get; init; }
    public ModemResultEnum ResultStatus { get; init; }
    public string ErrorMessage { get; init; }
}