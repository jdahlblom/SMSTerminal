using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;

namespace SMSTerminal.SMSMessages;

public class IncomingSms : IShortMessageService
{
    private string _message;
    
    public string MessageId { get; set; }
    public int ContactId { get; set; }
    public string SenderName { get; set; }
    public string SenderTelephone { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverTelephone { get; set; }
    public string ExternalTelephone { get; set; }
    public string ModemTelephone { get; set; }
    public SmsDirection Direction => SmsDirection.Incoming;
    public DateTimeOffset PDUTimeStamp { get; set; }
    public DateTime DateCreated => PDUTimeStamp.ToLocalTime().DateTime;
    /// <summary>
    /// If SMS-STATUS-REPORT it refers to when original SMS was sent 
    /// </summary>
    public DateTime DateSent => PDUTimeStamp.ToLocalTime().DateTime;
    /// <summary>
    /// When SMSTerminal received the message.
    /// </summary>
    public DateTime DateReceived { get; private set; }
    public SMSEncoding SMSEncoding { get; set; }
    public string SenderId { get; set; }
    public string RawString { get; set; }
    public string RawMessage { get; set; }
    public bool HasBeenRead { get; set; }
    public string Information { get; set; }
    public SMSEncoding SmsEncoding { get; set; }
    public bool IsStatusReport { get; set; }
    public DateTimeOffset StatusReportDischargeTimeStamp { get; set; }
    public int StatusReportReference { get; set; }
    public TpStatus StatusReportStatus { get; set; }
    public string FullPDUInformation { get; set; }

    public string ExternalName
    {
        get => string.IsNullOrEmpty(SenderName) ? "Unknown" : SenderName;
        set => SenderName = value;
    }

    public string Message
    {
        get
        {
            if (IsStatusReport)
            {
                return "This is a status report for the message sent on " +
                       DateSent +
                       $" to telephone {SenderTelephone}. Status for message sent is : {PDUFunctions.GetFriendlyTpStatusMessage(StatusReportStatus)}";
            }
            return _message ?? "";
        }
        set
        {
            if (!IsStatusReport)
            {
                _message = value;
            }
        }
    }

    public bool ContainsSearchString(string searchString)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            return false;
        }
        var search = searchString.ToUpperInvariant();
        if (!string.IsNullOrEmpty(SenderName) && SenderName.ToUpperInvariant().Contains(search))
        {
            return true;
        }
        if (!string.IsNullOrEmpty(SenderTelephone) && SenderTelephone.ToUpperInvariant().Contains(search))
        {
            return true;
        }
        if (!string.IsNullOrEmpty(ReceiverName) && ReceiverName.ToUpperInvariant().Contains(search))
        {
            return true;
        }
        if (!string.IsNullOrEmpty(ReceiverTelephone) && ReceiverTelephone.ToUpperInvariant().Contains(search))
        {
            return true;
        }
        if (!string.IsNullOrEmpty(Message) && Message.ToUpperInvariant().Contains(search))
        {
            return true;
        }
        return false;
    }

    private static IncomingSms ConvertToIncomingSms(IModemMessage modemMessage)
    {
        var incomingSms = new IncomingSms
        {
            ModemTelephone = modemMessage.ModemTelephone,
            SenderTelephone = modemMessage.Telephone,
            PDUTimeStamp = modemMessage.DateSent,
            DateReceived = modemMessage.DateReceived.ToLocalTime().DateTime,
            RawMessage = modemMessage.RawMessage,
            Message = modemMessage.Message,
            IsStatusReport = modemMessage.IsStatusReport,
            FullPDUInformation = modemMessage.FullPDUInformation
        };

        if (!modemMessage.IsStatusReport) return incomingSms;

        incomingSms.StatusReportDischargeTimeStamp = modemMessage.StatusReportDischargeTimeStamp;
        incomingSms.StatusReportReference = modemMessage.StatusReportReference;
        incomingSms.StatusReportStatus = modemMessage.StatusReportStatus;
        return incomingSms;
    }

    public static List<IncomingSms> Convert(IEnumerable<IModemMessage> modemMessageList)
    {
        var result = new List<IncomingSms>();
        foreach (var modemMessage in modemMessageList)
        {
            result.Add(Convert(modemMessage));
        }

        return result;
    }

    public static IncomingSms Convert(IModemMessage modemMessage)
    {
        return ConvertToIncomingSms(modemMessage);
    }

    public override string ToString()
    {
        return string.Format("Id: {0}" + Environment.NewLine +
                             "DateSent: {1}" + Environment.NewLine +
                             "HasBeenRead: {2}" + Environment.NewLine +
                             " Information: {3}" + Environment.NewLine +
                             " IsStatusReport: {4}" + Environment.NewLine +
                             " Message: {5}" + Environment.NewLine +
                             " RawString: {6}" + Environment.NewLine +
                             " SenderId: {7}" + Environment.NewLine +
                             " SenderName: {8}" + Environment.NewLine +
                             " SenderTelephone: {9}" + Environment.NewLine +
                             " SMSEncoding: {10}" + Environment.NewLine +
                             " StatusReportDischargeTimeStamp: {11}" + Environment.NewLine +
                             " StatusReportReference: {12}" + Environment.NewLine +
                             " StatusReportStatus: {13}" + Environment.NewLine +
                             " ModemTelephone: {14}",
            MessageId, DateSent, HasBeenRead, Information, IsStatusReport, Message,
            RawString, SenderId, SenderName, SenderTelephone, SmsEncoding,
            StatusReportDischargeTimeStamp, StatusReportReference, StatusReportStatus, ModemTelephone);

    }
}