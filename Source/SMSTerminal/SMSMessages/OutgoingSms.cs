using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;

namespace SMSTerminal.SMSMessages;

public class OutgoingSms : IShortMessageService
{
    private string _recipientTelephone;
    private string _pdu;

    public string MessageId { get; set; }
    public int ContactId { get; set; }
    public string SenderName { get; set; }
    public string SenderTelephone { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverTelephone
    {
        get => _recipientTelephone;
        set
        {
            if (!value.IsValidTph())
            {
                throw new ArgumentException($"Invalid telephone number : ->{value}<- \n{ToString()}");
            }
            _recipientTelephone = value.MakeValidTph();
        }
    }
    public string ModemTelephone { get; set; }
    public SMSEncoding SMSEncoding { get; set; } = SMSEncoding._7bit;
    public bool RequestStatusReport { get; set; }
    public SmsDirection Direction => SmsDirection.Outgoing;
    public DateTime DateCreated { get; set; }
    public DateTime DateSent { get; set; }
    public string Message { get; set; }
    public bool Processed { get; set; }
    public long MilliSecsToSend { get; set; }
    public string SendingModem { get; set; }
    public int BatchId { get; set; }
    public int Retries { get; set; }

    public void UpRetryCount()
    {
        Retries += 1;
    }

    public int ByteLength()
    {
        switch (SMSEncoding)
        {
            case SMSEncoding._7bit:
            case SMSEncoding._8bit:
                {
                    return System.Text.Encoding.ASCII.GetByteCount(Message);
                }
            case SMSEncoding._UCS2:
                {
                    return System.Text.Encoding.Unicode.GetByteCount(Message);
                }
            default:
                {
                    throw new Exception("Failed to switch SMSEncoding");
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

    public void SetUsedPDU(string[] pduArray)
    {
        var s = "";
        foreach (var pdu in pduArray)
        {
            s += pdu + Environment.NewLine;
        }
        PDU = s;
    }

    public string PDU
    {
        get => _pdu;
        set
        {
            if (!string.IsNullOrEmpty(value) && value.Length > 7900)
            {
                _pdu = value[..7900];
            }
            else
            {
                _pdu = value;
            }
        }
    }


    public override string ToString()
    {
        return string.Format(Environment.NewLine +
                             "Outgoing SMS" + Environment.NewLine +
                             "--------------------------------" + Environment.NewLine +
                             "BatchId: {0}" + Environment.NewLine +
                             "DateCreated: {1}" + Environment.NewLine +
                             "DateSent: {2}" + Environment.NewLine +
                             "Message: {3}" + Environment.NewLine +
                             "MilliSecsToSend: {4}" + Environment.NewLine +
                             "PDU: {5}" + Environment.NewLine +
                             "Processed: {6}" + Environment.NewLine +
                             "ContactId: {7}" + Environment.NewLine +
                             "ReceiverName: {8}" + Environment.NewLine +
                             "ReceiverTelephone: {9}" + Environment.NewLine +
                             "RequestStatusReport: {10}" + Environment.NewLine +
                             "Retries: {11}" + Environment.NewLine +
                             "SenderName: {12}" + Environment.NewLine +
                             "SenderTelephone: {13}" + Environment.NewLine +
                             "SendingModem: {14}" + Environment.NewLine +
                             "SMSEncoding: {15}" + Environment.NewLine +
                             "SMSId: {16}" + Environment.NewLine +
                             "--------------------------------" + Environment.NewLine, BatchId, DateCreated, DateSent, Message,
            MilliSecsToSend, _pdu, Processed, ContactId, ReceiverName, _recipientTelephone, RequestStatusReport, Retries, SenderName,
            SenderTelephone, SendingModem, SMSEncoding, MessageId);
    }
}