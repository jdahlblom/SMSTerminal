namespace SMSTerminal.PDU;

public enum SMSMessageType : byte
{
    SMS_DELIVER_INCOMING = 0,             //0 0 INCOMING SMS                0
    SMS_DELIVER_REPORT_OUTGOING = 1,      //0 0 OUTGOING REPORT             0
    SMS_SUBMIT = 2,                       //0 1 OUTGOING SMS                1
    SMS_SUBMIT_REPORT = 3,                //0 1 INCOMING SUBMIT REPORT      1
    SMS_STATUS_REPORT = 4,                //1 0 INCOMING STATUS REPORT      2
    SMS_COMMAND = 5,                      //1 0 OUTGOING SMS COMMAND        2
    RESERVED = 6                          //1 1 DO NOT USE                  3
}

public enum SMSType
{
    SMS = 0,
    StatusReport = 1
}

public enum ValidityPeriodFormat
{
    FieldNotPresent = 0,
    Enhanced = 0x08,
    Relative = 0x10,
    Absolute = 0x18
}

/// <summary>
/// AKA First Octet
/// </summary>
public class PDUHeader
{
    /*
     * INCOMING MESSAGE:
     * Bit no	7	    6	    5	    4	        3	        2	    1	    0
     * -------------------------------------------------------------------------------
     * Name	    TP-RP	TP-UDHI	TP-SRI	(unused)	(unused)	TP-MMS	TP-MTI	TP-MTI
     * 
     * Name	Meaning
     * TP-RP	Reply path. Parameter indicating that reply path exists.
     * TP-UDHI	User data header indicator. This bit is set to 1 if the User Data field starts with a header
     * TP-SRI	Status report indication. This bit is set to 1 if the sender wants a status report. (SME)
     * TP-MMS	More messages to send. This bit is set to 0 if there are more messages to send
     * TP-MTI	Message type indicator. Bits no 1 and 0 are both set to 0 to indicate that this PDU is an SMS-DELIVER 
     * 
     * --------------------------------------------------------------------------------------------------------------------------------------
     * --------------------------------------------------------------------------------------------------------------------------------------
     * --------------------------------------------------------------------------------------------------------------------------------------
     * 
     * OUTGOING MESSAGE:
     * Bit no	7	    6	    5	    4	    3	    2	    1	    0
     * Name	    TP-RP	TP-UDHI	TP-SRR	TP-VPF	TP-VPF	TP-RD	TP-MTI	TP-MTI
     * 
     * Fieldname	Meaning
     * TP-RP	Reply path. Parameter indicating that reply path exists.
     * TP-UDHI	User data header indicator. This bit is set to 1 if the User Data field starts with a header
     * TP-SRR	Status report request. This bit is set to 1 if a status report is requested
     * TP-VPF	Validity Period Format. Bit4 and Bit3 specify the TP-VP field according to this table:
     * bit4 bit3
     * 0 0 : TP-VP field not present
     * 1 0 : TP-VP field present. Relative format (one octet)
     * 0 1 : TP-VP field present. Enhanced format (7 octets)
     * 1 1 : TP-VP field present. Absolute format (7 octets)
     * TP-RD	Reject duplicates. Parameter indicating whether or not the SC shall accept an SMS-SUBMIT for an SM still held in the SC which has the same TP-MR and the same TP-DA as a previously submitted SM from the same OA.
     * TP-MTI	Message type indicator. Bits no 1 and 0 are set to 0 and 1 respectively to indicate that this PDU is an SMS-SUBMIT 
     * 
     * --------------------------------------------------------------------------------------------------------------------------------------
     * --------------------------------------------------------------------------------------------------------------------------------------
     * --------------------------------------------------------------------------------------------------------------------------------------
     * 
     *    bit 7 : TP-RP	    Reply path. Parameter indicating that reply path exists. (is only set by the SMSC)
     *    bit 6 : TP-UDHI	User data header indicator. This bit is set to 1 if the User Data field starts with a header
     *    bit 5 : TP-SRR	Status report request. This bit is set to 1 if a status report is needed
     *    bit 4 : VPF       Validity period
     *    bit 3 : VPF       Validity period
     *                            bit 4       bit 3
     *    (0x0)                     0           0     VP field is not present 
     *    (0x1)                     0           1     Reserved
     *    (0x2)                     1           0     VP field present an integer represented (relative)
     *    (0x3)                     1           1     VP field present an semi-octet represented (absolute) any reserved values may be rejected by the SMSC
     *    bit 2 : TP-RD 	Reject duplicates, 0 accept, 1 reject
     *    bit 1 : TP-MTI	Message type indicator. Bits no 1 and 0 are both set to 0 to indicate that this PDU is an SMS-DELIVER
     *    bit 0 : TP-MTI	
     *                              bit 1   bit 0
     *    (0x0)                        0    0    SMS-DELIVER (SMSC ==> MS) INCOMING SMS
     *    (0x0)                        0    0    SMS-DELIVER-REPORT (MS ==> SMSC) OUTGOING
     *    (0x1)                        0    1    SMS-SUBMIT (MS ==> SMSC) OUTGOING SMS
     *    (0x1)                        0    1    SMS-SUBMIT-REPORT (SMSC ==> MS) INCOMING STATUS REPLY WITH ERROR CAUSE
     *    (0x2)                        1    0    SMS-STATUS-REPORT (SMSC ==> MS) INCOMING STATUS REPORT
     *    (0x2)                        1    0    SMS-COMMAND (MS ==> SMSC) OUTGOING
     *    (0x3)                        1    1    Reserved
     */

    /*
     
        11111111
        ||| || |Message Type (Mask 0x3)
        ||| || 
        ||| ||Reject Duplicate Messages (Mask 0x4)
        ||| |
        ||| |Validity Period Format (Mask 0x18)
        |||
        |||Status Report Requested (Mask 0x20)
        ||
        ||User Data Header Exists (Mask 0x40)
        |
        |Reply Path Exists(Mask 0x80)
       
      
     *----------------------------------------------------------------------------------------------------------------------------------------
     *----------------------------------------------------------------------------------------------------------------------------------------
     *----------------------------------------------------------------------------------------------------------------------------------------

        RP:     0   Reply Path parameter is not set in this PDU
                1   Reply Path parameter is set in this PDU

        UDHI:   0   The UD field contains only the short message
                1   The beginning of the UD field contains a header in addition of the short message
        
        SRR:   0    A status report is not requested
               1    A status report is requested

        VPF:   bit4    bit3
               0       0     VP field is not present
               0       1     Reserved
               1       0     VP field present an integer represented (relative)
               1       1     VP field present an semi-octet represented (absolute) any reserved values may be rejected by the SMSC

        RD:    0   Instruct the SMSC to accept an SMS-SUBMIT for an short message still   
                   held in the SMSC which has the same MR and DA as a previously      
                   submitted short message from the same OA.
               1   Instruct the SMSC to reject an SMS-SUBMIT for a short message still
                   held in the SMSC which has the same MR and DA as a previously submitted short message from the same OA.

        MTI:   bit1    bit0    Message type
               **0       0       SMS-DELIVER (SMSC ==> MS)          INCOMING SMS
               **0       1       SMS-SUBMIT-REPORT (SMSC ==> MS)    INCOMING STATUS REPLY WITH ERROR CAUSE
               **1       0       SMS-STATUS-REPORT (SMSC ==> MS)    INCOMING STATUS REPORT
               -------------------------------------------------------------
               0         0       SMS-DELIVER-REPORT (MS ==> SMSC)   OUTGOING DELIVERY STATUS WITH ERROR CAUSE
               0         1       SMS-SUBMIT (MS ==> SMSC)           OUTGOING NORMAL SMS
               1         0       SMS-COMMAND (MS ==> SMSC)          OUTGOING SENDING A COMMAND
               1         1       Reserved 
     */




    //bit 7
    public bool ReplyPathExists { get; set; }

    //bit 6
    public bool UserDataHeaderExists { get; set; }

    //bit 5
    private bool _statusReportIndication; //incoming
    private bool _statusReportRequested;  //outgoing

    //bit 3 & 4 used for outgoing SMS only
    private ValidityPeriodFormat _validityPeriodFormat;

    //bit 2
    private bool _moreMessagesToSend;        //incoming
    private bool _rejectDuplicates = true;   //outgoing
        
    //bit 1 message type indicator

    public PDUHeader(MessageDirection messageDirection)
    {
        SmsMessageDirection = messageDirection;
    }

    public PDUHeader(MessageDirection messageDirection, byte firstOctet)
    {
        SmsMessageDirection = messageDirection;
        if (messageDirection == MessageDirection.INCOMING)
        {
            ParseIncomingFirstOctet(firstOctet);
        }
        else
        {
            ParseOutgoingFirstOctet(firstOctet);
        }
    }

    private void ParseIncomingFirstOctet(byte firstOctet)
    {
        /*
         * INCOMING MESSAGE:
         * Bit no	7	    6	    5	    4	        3	        2	    1	    0
         * -------------------------------------------------------------------------------
         * Name	    TP-RP	TP-UDHI	TP-SRI	(unused)	(unused)	TP-MMS	TP-MTI	TP-MTI
         * 
         * Name	Meaning
         * TP-RP	Reply path. Parameter indicating that reply path exists.
         * TP-UDHI	User data header indicator. This bit is set to 1 if the User Data field starts with a header
         * TP-SRI	Status report indication. This bit is set to 1 if a status report is going to be returned to the SME
         * TP-MMS	More messages to send. This bit is set to 0 if there are more messages to send
         * TP-MTI	Message type indicator. Bits no 1 and 0 are both set to 0 to indicate that this PDU is an SMS-DELIVER 
         */
        SetMessageType((byte)(firstOctet & 0x3));
        _moreMessagesToSend = (firstOctet & 0x4) == 0;
            
        _statusReportIndication = (firstOctet & 0x20) > 0;
        UserDataHeaderExists = (byte)(firstOctet & 0x40) > 1;
        ReplyPathExists = (byte)(firstOctet & 0x80) > 1;
    }

    private void ParseOutgoingFirstOctet(byte firstOctet)
    {
        /*
         * OUTGOING MESSAGE:
         * Bit no	7	    6	    5	    4	    3	    2	    1	    0
         * Name	    TP-RP	TP-UDHI	TP-SRR	TP-VPF	TP-VPF	TP-RD	TP-MTI	TP-MTI
         * 
         * Fieldname	Meaning
         * TP-RP	Reply path. Parameter indicating that reply path exists.
         * TP-UDHI	User data header indicator. This bit is set to 1 if the User Data field starts with a header
         * TP-SRR	Status report request. This bit is set to 1 if a status report is requested
         * TP-VPF	Validity Period Format. Bit4 and Bit3 specify the TP-VP field according to this table:
         * bit4 bit3
         * 0 0 : TP-VP field not present
         * 1 0 : TP-VP field present. Relative format (one octet)
         * 0 1 : TP-VP field present. Enhanced format (7 octets)
         * 1 1 : TP-VP field present. Absolute format (7 octets)
         * TP-RD	Reject duplicates. Parameter indicating whether or not the SC shall accept an SMS-SUBMIT for an SM still held in the SC which has the same TP-MR and the same TP-DA as a previously submitted SM from the same OA.
         * TP-MTI	Message type indicator. Bits no 1 and 0 are set to 0 and 1 respectively to indicate that this PDU is an SMS-SUBMIT 
         */
        SetMessageType((byte)(firstOctet & 0x3));
        _rejectDuplicates = (firstOctet & 0x4) > 0;
        _validityPeriodFormat = (ValidityPeriodFormat)(firstOctet & 0x18);
        _statusReportRequested = (firstOctet & 0x20) > 0;
        UserDataHeaderExists = (byte)(firstOctet & 0x40) > 1;
        ReplyPathExists = (byte)(firstOctet & 0x80) > 1;
    }

    private byte GetMessageType()
    {
        switch (SmsMessageType)
        {
            case SMSMessageType.SMS_DELIVER_INCOMING :
            case SMSMessageType.SMS_DELIVER_REPORT_OUTGOING:
            {
                return 0;
            }
            case SMSMessageType.SMS_SUBMIT:
            case SMSMessageType.SMS_SUBMIT_REPORT:
            {
                return 1;
            }
            case SMSMessageType.SMS_STATUS_REPORT:
            case SMSMessageType.SMS_COMMAND:
            {
                return 2;
            }
        }
        return 3;
    }

    private void SetMessageType(byte messageType)
    {
        switch (SmsMessageDirection)
        {
            case MessageDirection.INCOMING:
            {
                switch (messageType)
                {
                    case 0:
                    {
                        SmsMessageType = SMSMessageType.SMS_DELIVER_INCOMING;
                    }
                        break;
                    case 1:
                    {
                        SmsMessageType = SMSMessageType.SMS_SUBMIT_REPORT;
                    }
                        break;
                    case 2:
                    {
                        SmsMessageType = SMSMessageType.SMS_STATUS_REPORT;
                    }
                        break;
                    case 3:
                    {
                        SmsMessageType = SMSMessageType.RESERVED;
                    }
                        break;
                }
                break;
            }
            case MessageDirection.OUTGOING:
            {
                switch (messageType)
                {
                    case 0:
                    {
                        SmsMessageType = SMSMessageType.SMS_DELIVER_REPORT_OUTGOING;
                    }
                        break;
                    case 1:
                    {
                        SmsMessageType = SMSMessageType.SMS_SUBMIT;
                    }
                        break;
                    case 2:
                    {
                        SmsMessageType = SMSMessageType.SMS_COMMAND;
                    }
                        break;
                    case 3:
                    {
                        SmsMessageType = SMSMessageType.RESERVED;
                    }
                        break;
                }
                break;
            }
        }
    }

    public byte Octet()
    {
        if (SmsMessageDirection == MessageDirection.INCOMING)
        {
            return GetIncomingOctet();
        }
        return GetOutgoingOctet();
    }

    private byte GetIncomingOctet()
    {
        /*
         * INCOMING MESSAGE:
         * Bit no	7	    6	    5	    4	        3	        2	    1	    0
         * -------------------------------------------------------------------------------
         * Name	    TP-RP	TP-UDHI	TP-SRI	(unused)	(unused)	TP-MMS	TP-MTI	TP-MTI
         * 
         * Name	Meaning
         * TP-RP	Reply path. Parameter indicating that reply path exists.
         * TP-UDHI	User data header indicator. This bit is set to 1 if the User Data field starts with a header
         * TP-SRI	Status report indication. This bit is set to 1 if a status report is going to be returned to the SME
         * TP-MMS	More messages to send. This bit is set to 0 if there are more messages to send
         * TP-MTI	Message type indicator. Bits no 1 and 0 are both set to 0 to indicate that this PDU is an SMS-DELIVER 
         */
        var result = GetMessageType();
        if (_moreMessagesToSend)
        {
            result = (byte)(result | 0x04);
        }
        if (_statusReportIndication)
        {
            result = (byte)(result | 0x20);
        }
        if (UserDataHeaderExists)
        {
            result = (byte)(result | 0x40);
        }
        if (ReplyPathExists)
        {
            result = (byte)(result | 0x80);
        }
        return result;
    }

    private byte GetOutgoingOctet()
    {
        /*
         * OUTGOING MESSAGE:
         * Bit no	7	    6	    5	    4	    3	    2	    1	    0
         * Name	    TP-RP	TP-UDHI	TP-SRR	TP-VPF	TP-VPF	TP-RD	TP-MTI	TP-MTI
         * 
         * Fieldname	Meaning
         * TP-RP	Reply path. Parameter indicating that reply path exists.
         * TP-UDHI	User data header indicator. This bit is set to 1 if the User Data field starts with a header
         * TP-SRR	Status report request. This bit is set to 1 if a status report is requested
         * TP-VPF	Validity Period Format. Bit4 and Bit3 specify the TP-VP field according to this table:
         * bit4 bit3
         * 0 0 : TP-VP field not present
         * 1 0 : TP-VP field present. Relative format (one octet)
         * 0 1 : TP-VP field present. Enhanced format (7 octets)
         * 1 1 : TP-VP field present. Absolute format (7 octets)
         * TP-RD	Reject duplicates. Parameter indicating whether or not the SC shall accept an SMS-SUBMIT for an SM still held in the SC which has the same TP-MR and the same TP-DA as a previously submitted SM from the same OA.
         * TP-MTI	Message type indicator. Bits no 1 and 0 are set to 0 and 1 respectively to indicate that this PDU is an SMS-SUBMIT 
         */
        var result = GetMessageType();
        if (_rejectDuplicates)
        {
            result = (byte)(result | 0x04);
        }
        result = (byte)(result | (int)_validityPeriodFormat);
        if (_statusReportRequested)
        {
            result = (byte)(result | 0x20);
        }
        if (UserDataHeaderExists)
        {
            result = (byte)(result | 0x40);
        }
        if (ReplyPathExists)
        {
            result = (byte)(result | 0x80);
        }
        return result;
    }


    public bool StatusReportRequested
    {
        get
        {
            if (SmsMessageDirection == MessageDirection.INCOMING)
            {
                throw new MethodAccessException("[PDUHeader] StatusReportRequested is not valid when MessageDirection is INCOMING");
            }
            return _statusReportRequested;
        }
        set
        {
            if (SmsMessageDirection == MessageDirection.INCOMING)
            {
                throw new MethodAccessException("[PDUHeader] StatusReportRequested is not valid when MessageDirection is INCOMING");
            }
            _statusReportRequested = value;
        }
    }

    public ValidityPeriodFormat ValidityPeriodFormatUsed
    {
        get
        {
            if (SmsMessageDirection == MessageDirection.INCOMING)
            {
                throw new MethodAccessException("[PDUHeader] ValidityPeriodFormatUsed is not valid when MessageDirection is INCOMING");
            }
            return _validityPeriodFormat;
        }
        set
        {
            if (SmsMessageDirection == MessageDirection.INCOMING)
            {
                throw new MethodAccessException("[PDUHeader] ValidityPeriodFormatUsed is not valid when MessageDirection is INCOMING");
            }
            _validityPeriodFormat = value;
        }
    }

    public bool RejectDuplicates
    {
        get
        {
            if (SmsMessageDirection == MessageDirection.INCOMING)
            {
                throw new MethodAccessException("[PDUHeader] RejectDuplicates is not valid when MessageDirection is INCOMING");
            }
            return _rejectDuplicates;
        }
        set
        {
            if (SmsMessageDirection == MessageDirection.INCOMING)
            {
                throw new MethodAccessException("[PDUHeader] RejectDuplicates is not valid when MessageDirection is INCOMING");
            }
            _rejectDuplicates = value;
        }
    }

    public SMSMessageType SmsMessageType { get; set; } = SMSMessageType.SMS_SUBMIT;
        
    public MessageDirection SmsMessageDirection { get; set; }

    public bool MoreMessagesToSend
    {
        get { 
            if(SmsMessageDirection == MessageDirection.OUTGOING)
            {
                throw new MethodAccessException("[PDUHeader] MoreMessagesToSend is not valid when MessageDirection is OUTGOING");
            }
            return _moreMessagesToSend;
        }
        set
        {
            if (SmsMessageDirection == MessageDirection.OUTGOING)
            {
                throw new MethodAccessException("[PDUHeader] MoreMessagesToSend is not valid when MessageDirection is OUTGOING");
            }
            _moreMessagesToSend = value;
        }
    }

    public bool StatusReportIndication
    {
        get
        {
            if (SmsMessageDirection == MessageDirection.OUTGOING)
            {
                throw new MethodAccessException("[PDUHeader] StatusReportIndication is not valid when MessageDirection is OUTGOING");
            }
            return _statusReportIndication;
        }
        set
        {
            if (SmsMessageDirection == MessageDirection.OUTGOING)
            {
                throw new MethodAccessException("[PDUHeader] StatusReportIndication is not valid when MessageDirection is OUTGOING");
            }
            _statusReportIndication = value;
        }
    }

    public string ToOctet()
    {
        return Convert.ToString(Octet(), 2).PadLeft(8,'0');
    }

    public override string ToString()
    {
        if (SmsMessageDirection == MessageDirection.INCOMING)
        {
            return string.Format(Environment.NewLine + "PDUHeader Information for INCOMING SMS: " + Environment.NewLine + "---------------" + Environment.NewLine +
                                 "SMSMessageType: {0}" + Environment.NewLine +
                                 "ReplyPathExists: {1}" + Environment.NewLine +
                                 "StatusReportIndication: {2}" + Environment.NewLine +
                                 "MoreMessagesToSend: {3}" + Environment.NewLine +
                                 "UserDataHeaderExists: {4}" + Environment.NewLine + "---------------" + Environment.NewLine,
                SmsMessageType, ReplyPathExists, _statusReportIndication, _moreMessagesToSend, UserDataHeaderExists);
        }
        return string.Format(Environment.NewLine + "PDUHeader Information for OUTGOING SMS" + Environment.NewLine + "---------------" + Environment.NewLine +
                             "SMSMessageType: {0}" + Environment.NewLine +
                             "ReplyPathExists: {1}" + Environment.NewLine +
                             "StatusReportRequest: {2}" + Environment.NewLine +
                             "ValidityPeriodFormat: {3}" + Environment.NewLine +
                             "RejectDuplicates: {4}" + Environment.NewLine +
                             "UserDataHeaderExists: {5}" + Environment.NewLine + "---------------" + Environment.NewLine,
            SmsMessageType, ReplyPathExists, _statusReportRequested, _validityPeriodFormat, _rejectDuplicates, UserDataHeaderExists);
    }
}