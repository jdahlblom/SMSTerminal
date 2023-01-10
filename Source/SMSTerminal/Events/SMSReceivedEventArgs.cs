using SMSTerminal.Interfaces;

namespace SMSTerminal.Events;

public class SMSReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Simple version with the most relevant information
    /// </summary>
    public IShortMessageService ShortMessageService { get; init; }

    /// <summary>
    /// Contains technical information about the message
    /// </summary>
    public IModemMessage ModemMessage { get; init; }
}