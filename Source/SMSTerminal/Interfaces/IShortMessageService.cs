using SMSTerminal.General;
using SMSTerminal.PDU;

namespace SMSTerminal.Interfaces;

/// <summary>
/// Interface for outgoing and incoming SMS.
/// </summary>
public interface IShortMessageService
{
    string MessageId { get; set; }
    /// <summary>
    /// Can be set to any database id and
    /// used for either receiver or sender.
    /// </summary>
    int ContactId { get; set; }
    string SenderName { get; set; }
    string SenderTelephone { get; set; }
    string ReceiverTelephone { get; set; }
    string ReceiverName { get; set; }
    string ModemTelephone { get; set; }
    bool IsStatusReport { get;}
    DateTime DateCreated { get; }
    DateTime DateSent { get; }
    string Message { get; }
    SmsDirection Direction { get; }
    SMSEncoding SMSEncoding { get; set; }
    bool ContainsSearchString(string searchString);
    public string FullPDUInformation { get; set; }
}