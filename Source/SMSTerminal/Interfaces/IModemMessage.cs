using SMSTerminal.PDU;

namespace SMSTerminal.Interfaces;


/// <summary>
/// Interface for the modem SMS message.
/// </summary>
public interface IModemMessage
{
    string Message { get; }
    string ModemTelephone { get; }
    string Telephone { get; }
    /// <summary>
    /// If SMS-STATUS-REPORT then it is the timestamp when original SMS was sent.
    /// If SMS-SUBMIT then it is the Service Centre Time Stamp.
    /// </summary>
    DateTimeOffset DateSent { get; }
    /// <summary>
    /// When SMSTerminal received the message.
    /// </summary>
    DateTimeOffset DateReceived { get; }
    string RawMessage { get; }
    List<int> MemorySlots { get; }
    bool IsStatusReport { get; }
    int StatusReportReference { get; }
    DateTimeOffset StatusReportDischargeTimeStamp { get; }
    TpStatus StatusReportStatus { get; }
    bool DeletedFromTA { get; set; }
    public string FullPDUInformation { get; }
    string ToString();
}