﻿using NLog;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.PDU;

/// <summary>
/// This is the message as received from the modem. Use ToString()
/// to see all field information.
/// </summary>
internal class PDUMessage : IModemMessage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private string _unDecodedMessage;
    private string _readableMessage;

    public PDUHeader PDUHeader { get; set; }
    public PDUTypeOfAddress SMSCTypeOfAddress { get; set; }
    public PDUTypeOfAddress SenderTypeOfAddress { get; set; }
    public PDUProtocolIdentifier PDUProtocolIdentifier { get; set; }
    public PDUUserDataHeader PDUUserDataHeader { get; set; }
    public PDUDataCodingScheme PDUDataCodingScheme { get; set; }
    public bool HasBeenConcatenated { get; set; }
    public int StatusReportReference { get; set; }
    public TpStatus StatusReportStatus { get; set; } = TpStatus.TP_STATUS_NONE;

    /// <summary>
    /// If SMS-STATUS-REPORT then it is the timestamp when original SMS was sent.
    /// If SMS-SUBMIT then it is the Service Centre Time Stamp.
    /// </summary>
    public PDUTimeStamp PDUTimeStamp { get; set; }
    /// <summary>
    /// If SMS-STATUS-REPORT then it is the timestamp when original SMS was sent.
    /// If SMS-SUBMIT then it is the Service Centre Time Stamp.
    /// </summary>
    public DateTimeOffset DateSent => PDUTimeStamp.GetDateTimeOffset();

    /*
     * When SMSTerminal received the message.
     */
    public DateTimeOffset DateReceived { get; set; }
    public string RawMessage { get; set; } = "";
    public List<int> MemorySlots { get; set; } = new();

    public bool IsStatusReport => PDUHeader.SmsMessageType == SMSMessageType.SMS_STATUS_REPORT;
    public bool IsCMS => PDUUserDataHeader != null && PDUUserDataHeader.ContainsConcatenationInformationElement;
    /// <summary>
    /// After this period message can be deleted. Probably an orphan CSMS.
    /// </summary>
    private long MaxAgeMilliSecs { get; }
    public string ModemTelephone { get; set; }
    public bool DeletedFromTA { get; set; }
    public string FullPDUInformation  => ToString();
    /*
     * Specific PDU members:
     */

    /* SPECIFIC TO SMS-STATUS-REPORT*/
    public DateTimeOffset StatusReportDischargeTimeStamp => PDUStatusReportSMSDischargeTimeStamp.GetDateTimeOffset();
    /*IMPORTANT: THE PDU TIMESTAMP FOR SMS ORIGINALLY SENT IE THE SMS THE SMS-STATUS-REPORT CONCERNS IS STORED UNDER PDUTimeStamp*/
    public PDUTimeStamp PDUStatusReportSMSDischargeTimeStamp { get; set; }

    public PDUMessage(DateTimeOffset dateReceived, string rawMessage, long maxAgeMilliSecs)
    {
        DateReceived = dateReceived;
        RawMessage = rawMessage;
        MaxAgeMilliSecs = maxAgeMilliSecs;
    }

    public bool HasExpired()
    {
        return DateTimeOffset.Now.MilliSecsNowDTO() - DateReceived.MilliSecsNowDTO() > MaxAgeMilliSecs;
    }

    internal void AppendRawMessage(string rawMessage)
    {
        RawMessage = RawMessage + " (appended) " + rawMessage;
        if (RawMessage.Length > 7900)
        {
            RawMessage = RawMessage.Substring(0, 7900);
        }
    }

    public void AddMemorySlot(int memorySlot)
    {
        if (!MemorySlots.Exists(o => o == memorySlot))
        {
            MemorySlots.Add(memorySlot);
        }
    }

    public void AddMemorySlots(List<int> memorySlots)
    {
        foreach (var memorySlot in memorySlots.Where(memorySlot => !MemorySlots.Exists(o => o == memorySlot)))
        {
            MemorySlots.Add(memorySlot);
        }
    }

    public string UnDecodedMessage
    {
        get => _unDecodedMessage;
        set
        {
            _unDecodedMessage = value;
            DecodeMessage();
        }
    }

    private void DecodeMessage()
    {
        try
        {
            //Logger.Debug("Will now decode [udh] = ->{0}<- message ->{1}<-", PDUUserDataHeader.GetHeaderAsHexString(), _unDecodedMessage);
            Message = new PDUDecoder().Decode(PDUUserDataHeader, PDUDataCodingScheme.SMSEncoding, _unDecodedMessage);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.DecodeException());
        }
    }

    public string Message
    {
        get
        {
            /*if(string.IsNullOrEmpty(_unDecodedMessage))
            {
                return null;
            }*/
            if (PDUUserDataHeader != null && PDUUserDataHeader.ContainsNotSupportedInformationElements())
            {
                //Do not return binary information or information meant to be read by machines.
                switch (PDUUserDataHeader.InformationElementList[0].IEI)
                {
                    case IEIEnum.NS_Application_Port_Addressing_Scheme_8Bit:
                    case IEIEnum.NS_Application_Port_Addressing_Scheme_16Bit:
                    {
                        return "MMS not supported";// + Environment.NewLine + _readableMessage;
                    }
                    default:
                    {
                        return "Message format not supported. " + PDUUserDataHeader.InformationElementList[0].IEI + Environment.NewLine + _readableMessage;
                    }
                }
            }
            return _readableMessage;
        }
        set => _readableMessage = value;
    }

    public string Telephone
    {
        get => SenderTypeOfAddress.Number;
        set => SenderTypeOfAddress.Number = value;
    }

    public int PartsTotal
    {
        get
        {
            var informationElementList = PDUUserDataHeader.InformationElementList;
            if (informationElementList == null || informationElementList.Count == 0)
            {
                return 0;
            }
            foreach (var pduInformationElement in informationElementList)
            {
                if (pduInformationElement.IEI == IEIEnum.Concatenated_Short_Messages_8Bit_Reference ||
                    pduInformationElement.IEI == IEIEnum.Concatenated_Short_Messages_16Bit_Reference)
                {
                    return ((PDUIEICSMS)pduInformationElement).MessagePartsTotal;
                }
            }
            return 0;
        }
    }

    public int MessageReference
    {
        get
        {
            var informationElementList = PDUUserDataHeader?.InformationElementList;
            if (informationElementList == null || informationElementList.Count == 0)
            {
                return 0;
            }

            foreach (var pduInformationElement in informationElementList)
            {
                if (pduInformationElement.IEI is IEIEnum.Concatenated_Short_Messages_8Bit_Reference or IEIEnum.Concatenated_Short_Messages_16Bit_Reference)
                {
                    return ((PDUIEICSMS)pduInformationElement).MessageReference;
                }
            }
            return 0;
        }
    }

    public int ThisPart
    {
        get
        {
            var informationElementList = PDUUserDataHeader.InformationElementList;
            if (informationElementList == null || informationElementList.Count == 0)
            {
                return 0;
            }
            foreach (var pduInformationElement in informationElementList)
            {
                if (pduInformationElement.IEI == IEIEnum.Concatenated_Short_Messages_8Bit_Reference || pduInformationElement.IEI == IEIEnum.Concatenated_Short_Messages_16Bit_Reference)
                {
                    return ((PDUIEICSMS)pduInformationElement).ThisPart;
                }
            }
            return 0;
        }
    }

    public override string ToString()
    {
        return string.Format(Environment.NewLine +
                             Environment.NewLine + "----------------------------------" +
                             Environment.NewLine + "PDUModemMessage :" + Environment.NewLine +
                             "HasBeenProcessedForConcatenation: {0}" + Environment.NewLine +
                             "HasExpired: {1}" + Environment.NewLine +
                             "MemorySlot: {2}" + Environment.NewLine +
                             "PDUDataCodingScheme: {3}" + Environment.NewLine +
                             "PDUHeader: {4}" + Environment.NewLine +
                             "PDUProtocolIdentifier: {5}" + Environment.NewLine +
                             "PDUTimeStamp: {6}" + Environment.NewLine +
                             "PDUUserDataHeader: {7}" + Environment.NewLine +
                             "RawMessage: {8}" + Environment.NewLine +
                             "SenderTypeOfAddress: {9}" + Environment.NewLine +
                             "SMSCTypeOfAddress: {10}" + Environment.NewLine +
                             "UnDecodedMessage: {11}" + Environment.NewLine +
                             "PduStatusReportReference: {12}" + Environment.NewLine +
                             "StatusReportStatus: {13}" + Environment.NewLine +
                             "PduStatusReportSMSDischargeTimeStamp: {14}" + Environment.NewLine +
                             "Telephone : {15}" + Environment.NewLine +
                             "ReadableMessage : {16}" + Environment.NewLine +
                             "IsConcatenated : {17}" + Environment.NewLine +
                             "MessageReference : {18}" + Environment.NewLine +
                             "----------------------------------" + Environment.NewLine,
            HasBeenConcatenated, HasExpired(), string.Join(",", MemorySlots), PDUDataCodingScheme,
            PDUHeader, PDUProtocolIdentifier, PDUTimeStamp, PDUUserDataHeader,
            RawMessage, SenderTypeOfAddress, SMSCTypeOfAddress, _unDecodedMessage,
            StatusReportReference, StatusReportStatus, PDUStatusReportSMSDischargeTimeStamp, Telephone, Message, IsCMS, MessageReference);
    }
}