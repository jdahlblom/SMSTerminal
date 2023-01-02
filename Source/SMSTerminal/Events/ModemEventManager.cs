using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Events
{
    public static class ModemEventManager
    {
        /*********************************************************************************************************************/
        public delegate void ModemEventHandler(object sender, ModemEventArgs e);
        public static event ModemEventHandler OnModemEvent;

        /// <summary>
        /// Messages relating to the communication and modem configuration
        /// </summary>
        public static void ModemEvent(object sender, string modemId, string message, ModemEventType modemEventType, string id, ModemResultEnum resultStatus)
        {
            OnModemEvent?.Invoke(sender, new ModemEventArgs { 
                ModemId = modemId,
                Message = message,
                EventType = modemEventType, 
                Id = id,
                ResultStatus = resultStatus
            });
        }

        public static void AttachModemEventListener(IModemListener modemListener)
        {
            OnModemEvent += modemListener.ModemEvent;
        }

        public static void DetachModemEventListener(IModemListener modemListener)
        {
            OnModemEvent -= modemListener.ModemEvent;
        }
        
        /*********************************************************************************************************************/
        internal delegate void ModemMessageEventHandler(object sender, ModemMessageEventArgs e);
        internal static event ModemMessageEventHandler OnModemMessageEvent;

        /// <summary>
        /// Internal use for sending events about data from modem
        /// </summary>
        internal static void ModemMessageEvent(object sender, string modemId, ModemResultEnum modemResultEnum, ModemDataClassEnum modemDataClass, string data)
        {
            OnModemMessageEvent?.Invoke(sender, new ModemMessageEventArgs { 
                ModemId = modemId,
                ModemResult = modemResultEnum,
                ModemMessageClass = modemDataClass,
                Data = data
            });
        }

        internal static void AttachModemMessageListener(IModemMessageListener modemMessageListener)
        {
            OnModemMessageEvent += modemMessageListener.ModemMessageEvent;
        }

        internal static void DetachModemMessageListener(IModemMessageListener modemMessageListener)
        {
            OnModemMessageEvent -= modemMessageListener.ModemMessageEvent;
        }

        /*********************************************************************************************************************/
        public delegate void NewSMSEventHandler(object sender, SMSReceivedEventArgs e);
        public static event NewSMSEventHandler OnNewSMSEvent;

        /// <summary>
        /// Used when new SMS has been read
        /// </summary>
        public static void NewSMSEvent(object sender, IShortMessageService shortMessageService, IModemMessage modemMessage)
        {
            OnNewSMSEvent?.Invoke(sender, new SMSReceivedEventArgs()
            {
                ShortMessageService = shortMessageService, ModemMessage = modemMessage
            });
        }

        public static void AttachNewSMSListener(INewSMSListener newSMSListener)
        {
            OnNewSMSEvent += newSMSListener.NewSMSEvent;
        }

        public static void DetachNewSMSListener(INewSMSListener newSMSListener)
        {
            OnNewSMSEvent -= newSMSListener.NewSMSEvent;
        }
    }
}
