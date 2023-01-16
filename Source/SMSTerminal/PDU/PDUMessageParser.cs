using NLog;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.PDU;

/// <summary>
/// Parses raw modem output into single SMS.
/// </summary>
internal static class PDUMessageParser
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly object LockParseSingleRaw = new();
    private static readonly object LockParseIncomingStatusReport = new();
    private static readonly object LockParseIncomingSms = new();

    /// <summary>
    /// Used to add modem tph info to incoming SMS. So as to know
    /// which modem received it.
    /// </summary>
    public static string ModemTelephone { get; set; }= "";

    private static List<PDUMessage> _fragmentCSMSMessages = new();
    public static List<PDUMessage> FragmentCSMSMessages => _fragmentCSMSMessages;

    public static void MarkFragmentDeletedTA(int memorySlot)
    {
        foreach (var fragmentCSMSMessage in _fragmentCSMSMessages)
        {
            foreach (var slot in fragmentCSMSMessage.MemorySlots.Where(slot => slot == memorySlot))
            {
                fragmentCSMSMessage.DeletedFromTA = true;
            }
        }
    }

    internal static List<IModemMessage> ParseRawModemOutput(string rawModemOutput)
    {
        //AT+CMGF=0;+CMGL=4\r\n+CMGL: 1,1,"",159\r\n<PDU>\r\n\r\n\r\nOK\r\n

        /*
            This is how the raw output from the modem looks. A long SMS can be spread out over 
            several modem writes.

            AT+CMGF=0;+CMGL=4

            +CMGL: 1,0,,24
            <PDU>
            +CMGL: 2,0,,26
            <PDU>
            +CMGL: 3,0,,27
            <PDU>
            +CMGL: 4,0,,26
            <PDU>

            OK


         */


        //"AT+CMGF=0;+CMGL=0;+CMGL=1\r\r+CMGL: 1,1,\"\",23\r\r<pdu>\r\r\r\rOK\r\r"
        //"\r\r+CDS: 24\r\r\r\r<pdu>"

        var incomingPDUMessages = new List<PDUMessage>();

        if (rawModemOutput.Contains(ATCommands.ATReadUnreadSms) || rawModemOutput.Contains(ATCommands.ATReadReadSms))
        {
            ATCommands.RemoveSMSReadCommand(ref rawModemOutput);

            rawModemOutput = rawModemOutput.Replace(ATMarkers.OkReply, "");

            rawModemOutput = rawModemOutput.Trim();
            rawModemOutput = rawModemOutput.Replace(ATMarkers.MemoryStorage, $"@@@@@{ATMarkers.MemoryStorage}");


            var array = rawModemOutput.Split(new[] { "@@@@@" }, StringSplitOptions.RemoveEmptyEntries);
            array.TrimEntries();

            foreach (var s in array)
            {
                //Logger.Debug("About to parse ->{0}<-", s);
                try
                {
                    var pduModemMessage = ParseSingleRaw(s);
                    pduModemMessage.ModemTelephone = ModemTelephone;
                    incomingPDUMessages.Add(pduModemMessage);
                }
                catch (Exception e)
                {
                    //todo UGLY AS HELL
                    Logger.Error("Parse failed for raw modem data : ->{0}<-\n\n{1}", s, e.DecodeException());
                }
            }
        }
        else if (rawModemOutput.Contains(ATMarkers.NewStatusReportArrived))
        {
            try
            {
                var pduModemMessage = ParseSingleRaw(rawModemOutput);
                pduModemMessage.ModemTelephone = ModemTelephone;
                incomingPDUMessages.Add(pduModemMessage);
            }
            catch (Exception e)
            {
                //todo UGLY AS HELL
                Logger.Error("Parse failed for raw modem data : ->{0}<-\n\n{1}", rawModemOutput, e.DecodeException());
            }
        }

        List<PDUMessage> completePDUMessages = new();
        new PDUConcatenation().SortMessages(incomingPDUMessages, ref completePDUMessages, ref _fragmentCSMSMessages);
        return completePDUMessages.Cast<IModemMessage>().ToList();
    }

    private static PDUMessage ParseSingleRaw(string rawMessage)
    {
        lock (LockParseSingleRaw)
        {
            var pduModemMessage = new PDUMessage(DateTimeOffset.Now, rawMessage, ModemTimings.CSMSMaxAgeMilliSecs);

            var pduLengthInOctetsWithoutSMSC = 0;
            var pdu = "";

            if (rawMessage.Contains(ATMarkers.MemoryStorage))
            {
                var array = rawMessage.Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
                array.TrimEntries();
                if (array.Length != 2)
                {
                    throw new FormatException($"Failed to disassemble data from modem. {rawMessage}");
                }

                /*
                 * PreAmble = (+CMGL: <index>,<stat>,<length><CR><LF><pdu>) except the <pdu>
                 * PDU = <pdu>
                 */
                var preAmble = array[0];
                pdu = array[1];

                /*
                 *  MEMORY SLOT INDEX
                 */
                var memIndex = int.Parse(preAmble.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("+CMGL: ", ""));
                pduModemMessage.AddMemorySlot(memIndex);

                /*
                 *  PDU LENGTH
                 */
                var preAmbleArray = preAmble.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                pduLengthInOctetsWithoutSMSC = int.Parse(preAmbleArray[^1]);
            }
            else if (rawMessage.Contains(ATMarkers.NewStatusReportArrived))
            {
                rawMessage = rawMessage.Replace(ATMarkers.NewStatusReportArrived, "").Trim();
                var array = rawMessage.Split("\r", StringSplitOptions.RemoveEmptyEntries);
                pduLengthInOctetsWithoutSMSC = int.Parse(array[0]);
                pdu = array[1];
            }

            //See ParseIncomingStatusReport example of PDU structure
            var smcsLength = Convert.ToByte(pdu[..2], 16);

            if (smcsLength > 0)
            {
                var smcs = pdu.Substring(2, smcsLength * 2);

                /*
                 *  SMSC TypeOfAddress
                 */
                var smcsTypeOfAddress = new PDUTypeOfAddress(smcs);
                smcsTypeOfAddress.ParseOctets();
                pduModemMessage.SMSCTypeOfAddress = smcsTypeOfAddress;
            }

            pdu = pdu[^(pduLengthInOctetsWithoutSMSC * 2)..];

            /*
             * PDU HEADER
             */
            var pduHeader = new PDUHeader(MessageDirection.INCOMING, Convert.ToByte(pdu[..2], 16));
            pduModemMessage.PDUHeader = pduHeader;

            pdu = pdu[2..];

            pduModemMessage = pduHeader.SmsMessageType == SMSMessageType.SMS_STATUS_REPORT ?
                ParseIncomingStatusReport(pduModemMessage, pdu) :
                ParseIncomingSms(pduModemMessage, pdu);
            return pduModemMessage;
        }
    }

    private static PDUMessage ParseIncomingStatusReport(PDUMessage pduModemMessage, string pduAfterPDUHeader)
    {
        /*+CMGL: 1,0,,24
            0791534850020280061C0A814010431796114052011570211140520115702130

            07 91358405202008 06 1C 0A 81 4010431796 11405201157021 11405201157021 30
            smsc length
               smsc number
                              first octet
                                 Reference
                                    ->For number  <-
                                                     Timestamp
                                                                    Discharge time
                                                                                   Status
            ----------------------------------------------------------------------------------------
            Status report
            For number: 	0401347169
            Status: 	Unknown status
            Reference: 	28
            PDU type: 	SMS-STATUS-REPORT
            Time stamp: 	25/04/2011 10:51:07
            Discharge: 	25/04/2011 10:51:07
            SMSC: 	+358405202008

            SMSC: 		0791534850020280
            PDU header: 	06 		first octet                
                    TP-MTI: 	02 		message type indicator 
                    TP-MMS: 	04 		more messages to send
                    TP-SRQ: 	00 		status report qualifier, bit 5, 0 = The status report is the result of an SMS-SUBMIT, 1 = The status report is the result of an SMS-COMMAND
                    TP-UDHI: 	00 		user data header

            Reference   1C = 28d
            For number length = 0A  = 4010431796 and of type 81
            TP-RA: 		0A814010431796 	recipient address
            TP-SCTS: 	11405201157021 	service center timestamp when previous message was sent
            TP-DT: 		11405201157021 	discharge time
            TP-ST: 		30 		status
        */
        lock (LockParseIncomingStatusReport)
        {
            /*
             * MESSAGE REFERENCE
             */
            var reference = Convert.ToByte(pduAfterPDUHeader[..2], 16);
            pduModemMessage.StatusReportReference = reference;
            pduAfterPDUHeader = pduAfterPDUHeader[2..];

            /*
             * TELEPHONE NUMBER FOR RECIPIENT CONCERNED LENGTH
             */
            var concernedRecipientTypeOfAddressLength = Convert.ToByte(pduAfterPDUHeader[..2], 16);
            pduAfterPDUHeader = pduAfterPDUHeader[2..];

            /*
             * TELEPHONE NUMBER FOR RECIPIENT CONCERNED
             */
            var concernedRecipientTypeOfAddress = new PDUTypeOfAddress(pduAfterPDUHeader[..(concernedRecipientTypeOfAddressLength + 2)]); // +2 because of 0x81 / 0x91
            concernedRecipientTypeOfAddress.ParseOctets();
            //Logger.Debug("concernedRecipientTypeOfAddress {0}", concernedRecipientTypeOfAddress);
            pduModemMessage.SenderTypeOfAddress = concernedRecipientTypeOfAddress;
            pduAfterPDUHeader = pduAfterPDUHeader[(concernedRecipientTypeOfAddressLength + 2)..];
            //Logger.Debug("userPduSubStringed 3 {0}", userPduSubStringed);

            /*
             *  PDU TIMESTAMP WHEN SMS WAS ORIGINALLY SENT
             */
            var pduTimeStampWhenOriginallySent = new PDUTimeStamp(pduAfterPDUHeader[..14]);
            pduModemMessage.PDUTimeStamp = pduTimeStampWhenOriginallySent;
            //Logger.Debug("_pduTimeStamp (SMS originally sent) {0}", pduTimeStampWhenOriginallySent);
            pduAfterPDUHeader = pduAfterPDUHeader[14..];
            //Logger.Debug("userPduSubStringed 4 {0}", userPduSubStringed);

            /*
             *  PDU TIMESTAMP WHEN SC DELIVERED OR TRIED TO DELIVER OR DISPOSED THE PREVIOUS SMS
             */
            var pduTimeStampDischarge = new PDUTimeStamp(pduAfterPDUHeader[..14]);
            pduModemMessage.PDUStatusReportSMSDischargeTimeStamp = pduTimeStampDischarge;
            //Logger.Debug("pduTimeStampDischarge {0}", pduTimeStampDischarge);
            pduAfterPDUHeader = pduAfterPDUHeader[14..];
            //Logger.Debug("userPduSubStringed 5 {0}", userPduSubStringed);

            /*
             * STATUS REPORT STATUS
             */
            var statusReportStatus = TpStatus.TP_STATUS_NONE;
            try
            {
                statusReportStatus = (TpStatus)int.Parse(pduAfterPDUHeader[..2]);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to parse SMS-STATUS-REPORT tp-status to enum. {0}", ex.DecodeException());
            }
            pduModemMessage.StatusReportStatus = statusReportStatus;

            return pduModemMessage;
        }
    }



    private static PDUMessage ParseIncomingSms(PDUMessage pduModemMessage, string pduAfterPDUHeader)
    {
        lock (LockParseIncomingSms)
        {
            /*
             * SENDER ADDRESS LENGTH
             */
            var senderAddressLength = Convert.ToByte(pduAfterPDUHeader[..2], 16);
            if (senderAddressLength % 2 > 0)
            {
                //If the length of the tph number is odd an trailing F is added. So the length is in the pdu is the actual length + 1 (the F) but the value if the length without the F.
                senderAddressLength++;
            }
            //Logger.Debug("senderAddressLength {0}", senderAddressLength);
            pduAfterPDUHeader = pduAfterPDUHeader[2..];
            //Logger.Debug("userPduSubStringed 1i {0}", userPduSubStringed);
            /*
             * SENDER TypeOfAddress
             */
            var senderTypeOfAddress = new PDUTypeOfAddress(pduAfterPDUHeader[..(senderAddressLength + 2)]); // +2 because of 0x81 / 0x91
            senderTypeOfAddress.ParseOctets();

            //Logger.Debug("senderTypeOfAddress {0}", senderTypeOfAddress);
            pduModemMessage.SenderTypeOfAddress = senderTypeOfAddress;
            pduAfterPDUHeader = pduAfterPDUHeader[(senderAddressLength + 2)..];
            //Logger.Debug("userPduSubStringed 2i {0}", userPduSubStringed);
            /*
             * PDU PROTOCOL IDENTIFIER
             */
            var pduProtocolIdentifier = new PDUProtocolIdentifier(Convert.ToByte(pduAfterPDUHeader[..2], 16));
            pduModemMessage.PDUProtocolIdentifier = pduProtocolIdentifier;
            //Logger.Debug("pduProtocolIdentifier {0}", pduProtocolIdentifier);
            pduAfterPDUHeader = pduAfterPDUHeader[2..];
            //Logger.Debug("userPduSubStringed 3i {0}", userPduSubStringed);
            /*
             *  PDU DATA CODING SCHEME
             */
            var pduDataCodingScheme = new PDUDataCodingScheme(Convert.ToByte(pduAfterPDUHeader[..2], 16));
            pduModemMessage.PDUDataCodingScheme = pduDataCodingScheme;
            //Logger.Debug("pduDataCodingScheme {0}", pduDataCodingScheme);
            pduAfterPDUHeader = pduAfterPDUHeader[2..];
            //Logger.Debug("userPduSubStringed 4i {0}", userPduSubStringed);
            /*
             *  PDU TIMESTAMP (Service Centre Time Stamp)
             */
            var pduTimeStamp = new PDUTimeStamp(pduAfterPDUHeader[..14]);
            pduModemMessage.PDUTimeStamp = pduTimeStamp;
            //Logger.Debug("pduTimeStamp {0}", pduTimeStamp);
            pduAfterPDUHeader = pduAfterPDUHeader[14..];
            //Logger.Debug("userPduSubStringed 5i {0}", userPduSubStringed);
            /*
             * USER DATA LENGTH 
             */
            //var userDataLength = Convert.ToByte(pduAfterPDUHeader[..2], 16);
            //Logger.Debug("userDataLength {0}", userDataLength);
            pduAfterPDUHeader = pduAfterPDUHeader[2..];
            //Logger.Debug("userPduSubStringed 6i {0}", userPduSubStringed);
            /*
             * 
             */
            if (!pduModemMessage.PDUHeader.UserDataHeaderExists)
            {
                pduModemMessage.UnDecodedMessage = pduAfterPDUHeader;
                //Logger.Debug("UnDecodedMessage no UDH {0}", userPduSubStringed);
            }
            else
            {
                var userDataHeader = new PDUUserDataHeader(pduAfterPDUHeader);
                pduModemMessage.PDUUserDataHeader = userDataHeader;
                if (!userDataHeader.ParseUserDataHeader())
                {
                    Logger.Error("Error parsing UserDataHeader ->{0}<-", pduAfterPDUHeader);
                    if (userDataHeader.HasParseException)
                    {
                        Logger.Error(userDataHeader.LastParseException.DecodeException());
                    }
                }
                //Logger.Debug("UDH is {0}", udh);
                pduModemMessage.UnDecodedMessage = userDataHeader.MessagePartUndecoded;
                //Logger.Debug("UnDecodedMessage {0}", udh.MessagePartUndecoded);
            }
            //Logger.Debug("userPduSubStringed 7i {0}", userPduSubStringed);

            return pduModemMessage;
        }
    }
}