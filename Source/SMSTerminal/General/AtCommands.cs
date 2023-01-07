using NLog;

namespace SMSTerminal.General
{
    internal static class ATMarkers
    {
        public const string MemoryStorage = "+CMGL: "; //This is included when querying for SMS and there exists SMS
        public const string NewSMSArrivedSM = "+CMTI: \"SM\""; //This comes when there is new SMS. (unsolicited message)
        public const string NewSMSArrivedMT = "+CMTI: \"MT\""; //This comes when there is new SMS. (unsolicited message)
        public const string NewSMSArrivedME = "+CMTI: \"ME\""; //This comes when there is new SMS. (unsolicited message)
        
        public const string SMSDeleted = "AT+CMGD="; //This comes when there SMS has been deleted todo memory slot information
        public const string SMSMessageFormat = "AT+CMGF="; //Modem reply contains this after initializing the modem to receive SMS data 
        public const string SMSSent = "+CMGS: "; //Modem reply contains this after SMS transmission
        public const string NetworkStatusRequestCommand = "AT+CREG?";
        public const string NetworkInformation = "+CREG: "; //Modem reply contains this after querying network information
        public const string ReadyPrompt = "> "; //Modem is ready for the SMS data

        //***********************************************************************
        public const string OkReply = "\rOK";
        public const string CMSErrorReply = "\r+CMS ERROR: ";
        public const string CMEErrorReply = "\r+CME ERROR: ";
        public const string ErrorReply = "\rERROR";
        public const string CMSErrorKeyword = "+CMS ERROR:";
        public const string CMEErrorKeyword = "+CME ERROR:";
        public const string ErrorKeyword = "ERROR";
    }

    internal static class ATCommands
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /*
         * With this activated the AT+CNMI= will work (remember to set LineSignalDtr = true)
         * AT+CSMS=1 GSM 07.05 Phase 2+ version compatibility
         * AT+CNMI New short Message Indication unsolicited
         */
        //public const string AtGSMPhase2Command = "AT+CSMS=1;+CNMI=2,2,0,2,1";
        public const string ATGSMPhase2Command = "AT+CSMS=1;+CNMI=2,1,0,2,1";  // 2nd changes format of unsolicited SMS message
        public const string UseVerboseErrorsCommand = "AT+CMEE=2";
        //***********************************************************************
        public const string ATEndPart = "\r\n";
        public const string ATCarriageReturn = "\r";
        //***********************************************************************
        public const string ATSendSmsPDU = "AT+CMGF=0;+CMGS=";
        //***********************************************************************
        public const string ATReadAllSms = "AT+CMGF=0;+CMGL=0;+CMGL=1"; // PDU mode and read all SMS  AT+CMGF=0;+CMGL=0;+CMGL=1
        public const string ATReadUnreadSms = "AT+CMGF=0;+CMGL=0"; // PDU mode and read unread SMS  AT+CMGF=0;+CMGL=0;+CMGL=1
        public const string ATReadReadSms = "AT+CMGF=0;+CMGL=1"; // PDU mode and read read SMS
        //public const string AtUnsolicitedNewSMSAck = "AT+CNMA=0\r\n"; // 
        //***********************************************************************
        public const string ATDeleteAllReadSms = "AT+CMGD=1,1";
        public const string ATDeleteAllSmsFromModem = "AT+CMGD=1,4";
        public const string ATDeleteSmsAtMemorySlot = "AT+CMGD=";
        //***********************************************************************
        public const char ATCommandCtrlZ = '\u001A'; // Was string ((char)26).ToString(); //U+001A   '\u001A'
        public const char ATCommandEscape = '\u001B'; //was string ((char)27).ToString(); //U+001B  '\u001B'
        //***********************************************************************
        public const string ATIncomingCallIndicator = "RING";
        public const string ATIncomingCallIndicator2 = "+CRING";
        //***********************************************************************
        public const string ATDisconnectIncomingCallCommand = "ATH";
        //***********************************************************************
        public const string ATQueryCommand = "AT\r";
        //***********************************************************************
        //AT+CCFC=0,3,"+358...",145
        public const string ATEnableCallForwardingCommandPart1 = "AT+CCFC=0,3,\"";
        public const string ATEnableCallForwardingCommandPart2 = "\",145";
        public const string ATDisableCallForwardingCommand = "AT+CCFC=0,0";
        public const string ATQueryCallForwardingStatusCommand = "AT+CCFC=0,2";
        //***********************************************************************
        public const string ATNetworkStatusRequestCommand = "AT+CREG?";
        public const string ATPINStatusQueryCommand = "AT+CPIN?";
        public const string ATPINAuthCommand = "AT+CPIN=";
        //***********************************************************************
        public const string ATForceError = "AT+ABCD";
        //***********************************************************************
        public const string ATRestartModem = "AT+CFUN=1,1";
        //***********************************************************************


        private static readonly object LockContainsResultCode = new();
        internal static bool ContainsEscapeChars(string s)
        {
            lock (LockContainsResultCode)
            {
                return s.Contains(ATCommandCtrlZ)||
                       s.Contains(ATCommandEscape);
            }
        }
        
        internal static bool ContainsResultCode(string s)
        {
            lock (LockContainsResultCode)
            {
                return s.Contains(ATMarkers.OkReply) ||
                       s.Contains(ATMarkers.CMEErrorReply) ||
                       s.Contains(ATMarkers.CMSErrorReply) ||
                       s.Contains(ATMarkers.ErrorReply);
            }
        }

        public static bool ContainsSMSReadCommand(string data)
        {
            return data.Contains(ATReadUnreadSms) ||
                   data.Contains(ATReadReadSms) ||
                   data.Contains(ATReadAllSms);
        }

        public static void RemoveSMSReadCommand(ref string data)
        {
            data = data.Replace(ATReadUnreadSms, "")
                .Replace(ATReadReadSms, "")
                .Replace(ATReadAllSms, "");
        }

        //***********************************************************************

        //+CMTI: "MT",7  < message arrived and is located there


        //public readonly string AtSendSmsP1 = "AT+CMGF=1;+CSCS=\"8859-1\";+CMGS=\"";
        //public readonly string _atTurnOffRssi = "AT^CURC=0\r\n";, turn off ^RSSI:XX messages (Received signal strength indication) (Turn off Unsolicited Report Codes)
        /*

        AT+CNMI?
        AT+CSMS=1
        AT+CNMI=3,3,0,2,1        

        

         * Network status
         * AT+CGREG?
         *
         * ‘+CGREG:0,1’ implies that the device is registered and it is in the nime network (ie not roaming)            
            0,0 – not registered, MT is not currently searching a new operator to register to
            0,1 – Registered, home network
            0,2 – Searching
            0,3 – Registration denied
            0,5 – Registered, non-home network 
         */
    }
}
