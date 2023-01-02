using SMSTerminal.General;
using SMSTerminal.PDU;

namespace SMSTerminal.Interfaces
{
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
        string ReceiverName { get; }

        string ModemTelephone { get; set; }

        DateTime DateCreated { get; }
        DateTime DateSent { get; }
        string Message { get; set; }
        SmsDirection Direction { get; }
        SMSEncoding SMSEncoding { get; set; }
        bool ContainsSearchString(string searchString);
    }
}
