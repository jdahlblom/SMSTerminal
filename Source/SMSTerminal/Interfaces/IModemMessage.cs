using SMSTerminal.PDU;

namespace SMSTerminal.Interfaces;

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
    string ToString();
}