using System.IO.Ports;
using System.Threading.Channels;
using SMSTerminal.Commands;
using SMSTerminal.General;
using SMSTerminal.Modem;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.Interfaces;

/// <summary>
/// Modem interface. Contains minimum methods and members needed.
/// </summary>
internal interface IModem
{
    string ModemId { get; }
    /// <summary>
    /// Halts further operations in order not to lock the SIM card
    /// by applying wrong PIN multiple times.
    /// </summary>
    bool HaltForPINError { get; set; }
    SerialPort SerialPort { get; }
    Task<bool> ExecuteCommand(ICommand command);
    Task<bool> WriteTextData(string text);
    GsmModemConfig GsmModemConfig { get; }
    ISerialReceiver SerialReceiver { set; }
    Task<bool> SendSMS(OutgoingSms outgoingSms);
    Task<bool> ReadSMS(SMSReadStatus smsReadStatus);
    bool SendNewMessageAcknowledgement { get; set; }
    Signals Signals { get; }
    void Dispose();

}