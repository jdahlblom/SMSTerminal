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
    DateTimeOffset DateSent { get; }
    DateTimeOffset DateReceived { get; }
    string RawMessage { get; }
    List<int> MemorySlots { get; }
    bool IsStatusReport { get; }
    int StatusReportReference { get; }
    DateTimeOffset StatusReportDischargeTimeStamp { get; }
    TpStatus StatusReportStatus { get; }
    bool DeletedFromTA { get; set; }
    public string FullPDUInformation { get; set; }
    string ToString();
}