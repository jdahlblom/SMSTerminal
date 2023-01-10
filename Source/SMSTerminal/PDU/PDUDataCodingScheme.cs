namespace SMSTerminal.PDU;

//----------------------------
//Data Coding Scheme (TP-DCS)
//----------------------------
public enum CodingGroup : byte
{
    GENERAL_DATA_CODING_INDICATION = 0x30,//LESS THAN TESTING
    MESSAGE_WAITING_INDICATION_DISCARD = 0xC0,
    MESSAGE_WAITING_INDICATION_STORE = 0xD0,
    MESSAGE_WAITING_INDICATION_STORE_UCS2_UNCOMPRESSED = 0xE0,
    DATA_CODING_MESSAGE_CLASS = 0xF0,
    OTHER = 0
}

public enum SMSEncoding
{
    _7bit = 0,
    _8bit = 0x04 /*0100*/,
    _UCS2 = 0x08, /*1000*/
    ReservedMask = 0x0C /*1100*/
}

public enum MessageClass : byte
{
    MESSAGE_CLASS_0_ALERT = 0,  //Class 0: Indicates that this message is to be displayed on the MS immediately and a message delivery report is to be sent back to the SC. The message does not have to be saved in the MS or on the SIM card (unless selected to do so by the mobile user).
    MESSAGE_CLASS_1_ME = 1,     //Class 1: Indicates that this message is to be stored in the MS memory or the SIM card (depending on memory availability).
    MESSAGE_CLASS_2_SIM = 2,    //Class 2: This message class is Phase 2 specific and carries SIM card data. The SIM card data must be successfully transferred prior to sending acknowledgement to the SC. An error message will be sent to the SC if this transmission is not possible.
    MESSAGE_CLASS_3_TE = 3,     //Class 3: Indicates that this message will be forwarded from the receiving entity to an external device. The delivery acknowledgement will be sent to the SC regardless of whether or not the message was forwarded to the external device. 
    MESSAGE_CLASS_NONE = 99     //ADDED BY JD
}

public enum TypeOfMessageWaiting : byte
{
    VOICEMAIL = 0,
    FAX = 1,
    ELECTRONIC_MESSAGE = 2,
    OTHER = 3
}

//----------------------------
//End Data Coding Scheme (TP-DCS)
//----------------------------

public class PDUDataCodingScheme //Data Coding Scheme (TP-DCS)
{

    /*
     * GSM 03.40
     * 
     * Coding groups bits 7..4
     * 
     * 00xx         General Data Coding  I n d i c a t i o n        (Mask 0xC0) bit 7 & 6
     *              00000000
     *                || | | 0 0 Class 0 (Alert)                    (Mask 0x3)
     *                || | | 0 1 Class 1 ME Specific
     *                || | | 1 0 Class 2 SIM Specific
     *                || | | 1 1 Class 3 TE Specific
     *                || |
     *                || |  0 0 Default Alphabet                    (Mask 0x4)
     *                || |  0 1 8bit data
     *                || |  1 0 UCS2 (16bit)
     *                || |  1 1 Reserved
     *                ||
     *                ||  Bits 0 & 1 have a message class meaning   (Mask 0x10)  _messageClassBitsSet
     *                |
     *                | Text is compressed                          (Mask 0x20)
     * 
     * 
     * 0100..1011   Reserved Coding Groups (Mask 0x40 - 0xB0)
     * 
     * 1100         Message Waiting Indication Group: Discard Message (Mask 0xC0)
     *              Same as 1101 but the message does not have to be stored
     * 
     * 1101         Message Waiting Indication Group: Store Message (Mask 0xD0)
     *              11010000
     *                  || | 0 0 Voicemail Message Waiting
     *                  || | 0 1 Fax Message Waiting
     *                  || | 1 0 Electronic Mail Message Waiting
     *                  || | 1 1 Other Message Waiting*
     *                  ||
     *                  ||  Reserved, must be 0
     *                  |
     *                  |   Set Indication Active/Inactive
     * 
     * 
     * 1110         Message Waiting Indication Group: Store Message (Mask 0xE0)
     *              Same as 1101 byt the user data is coded in the uncompressed UCS2 alphabet.
     *              
     * 
     * 1111         Data coding/message class (Mask 0xF0)
     *              11110000
     *                  || | 0 0 Class 0 (Alert)                    (Mask 0x3)
     *                  || | 0 1 Class 1 ME Specific
     *                  || | 1 0 Class 2 SIM Specific
     *                  || | 1 1 Class 3 TE Specific
     *                  ||
     *                  ||  Default Alphabet / 8bit data    (Mask 0x4)
     *                  |         
     *                  | Reserved, must be 0
     *                
     * 
     * 
     * NOTE: The special case of bits 7..0 being 0000 0000 indicates the Default Alphabet as in Phase 2
     */


    public PDUDataCodingScheme(byte octet)
    {
        ParseOctet(octet);
    }

    private void ParseOctet(byte octet)
    {
        //NOTE: The special case of bits 7..0 being 0000 0000 indicates the Default Alphabet as in Phase 2
        if(octet == 0)
        {
            SMSEncoding = SMSEncoding._7bit;
            return;
        }

        //Test for General Data Coding
        var codingGroup = octet & 0xF0;
        if(codingGroup < 0x30)
        {
            CodingGroup = CodingGroup.GENERAL_DATA_CODING_INDICATION;
        }else if(codingGroup >= 0x40 && codingGroup <= 0xB0)
        {
            CodingGroup = CodingGroup.OTHER;
        }else if(codingGroup == 0xC0)
        {
            CodingGroup = CodingGroup.MESSAGE_WAITING_INDICATION_DISCARD;
        }else if(codingGroup == 0xD0)
        {
            CodingGroup = CodingGroup.MESSAGE_WAITING_INDICATION_STORE;
        }else if(codingGroup == 0xE0)
        {
            CodingGroup = CodingGroup.MESSAGE_WAITING_INDICATION_STORE_UCS2_UNCOMPRESSED;
        }else if(codingGroup == 0xF0)
        {
            CodingGroup = CodingGroup.DATA_CODING_MESSAGE_CLASS;
        }

        switch (CodingGroup)
        {
            case CodingGroup.GENERAL_DATA_CODING_INDICATION :
            {
                MessageClassBitsSet = (octet & 0x10) > 0;
                if(MessageClassBitsSet)
                {
                    MessageClass = (MessageClass) (octet & 0x3);
                }
                SMSEncoding = (SMSEncoding) (octet & 0xC);
                TextIsCompressed = (octet & 0x20) > 0;
                break;
            }
            case CodingGroup.MESSAGE_WAITING_INDICATION_DISCARD :
            case CodingGroup.MESSAGE_WAITING_INDICATION_STORE_UCS2_UNCOMPRESSED :
            case CodingGroup.MESSAGE_WAITING_INDICATION_STORE :
            {
                TypeOfMessageWaiting = (TypeOfMessageWaiting) (octet & 0x3);
                MessageWaitingIndicationActive = (octet & 0x8) > 0;
                break;
            }
            case CodingGroup.DATA_CODING_MESSAGE_CLASS :
            {
                MessageClass = (MessageClass) (octet & 0x3);
                SMSEncoding = (octet & 0x4) > 0 ? SMSEncoding._8bit : SMSEncoding._7bit;
                break;
            }
        }
    }

    public byte Octet()
    {
        byte result = 0;
        switch (CodingGroup)
        {
            case CodingGroup.GENERAL_DATA_CODING_INDICATION:
            {
                if (MessageClassBitsSet)
                {
                    result = (byte)(result | 0x10);
                }
                if (MessageClassBitsSet)
                {
                    result = (byte)(result | (byte)MessageClass);
                }
                result = (byte)(result | (byte)SMSEncoding);
                if(TextIsCompressed)
                {
                    result = (byte)(result | 0x20);    
                }
                break;
            }
            case CodingGroup.MESSAGE_WAITING_INDICATION_DISCARD:
            case CodingGroup.MESSAGE_WAITING_INDICATION_STORE_UCS2_UNCOMPRESSED:
            case CodingGroup.MESSAGE_WAITING_INDICATION_STORE:
            {
                result = (byte)(result | (byte)TypeOfMessageWaiting);
                if(MessageWaitingIndicationActive)
                {
                    result = (byte)(result | 0x8);
                }
                break;
            }
            case CodingGroup.DATA_CODING_MESSAGE_CLASS:
            {
                result = (byte)(result | (byte)MessageClass);
                if(SMSEncoding == SMSEncoding._8bit)
                {
                    result = (byte)(result | 0x4);
                }
                break;
            }
        }

        switch (CodingGroup)
        {
            case CodingGroup.MESSAGE_WAITING_INDICATION_DISCARD:
            case CodingGroup.MESSAGE_WAITING_INDICATION_STORE_UCS2_UNCOMPRESSED:
            case CodingGroup.MESSAGE_WAITING_INDICATION_STORE:
            case CodingGroup.DATA_CODING_MESSAGE_CLASS:
            {
                result = (byte)(result | (byte)CodingGroup);
                break;
            }
        }
        return result;
    }

    public CodingGroup CodingGroup { get; set; }
    public SMSEncoding SMSEncoding { get; set; }
    public MessageClass MessageClass { get; set; } = MessageClass.MESSAGE_CLASS_NONE;
    public bool TextIsCompressed { get; set; }
    public bool MessageClassBitsSet { get; set; }
    public bool MessageWaitingIndicationActive { get; set; }
    public TypeOfMessageWaiting TypeOfMessageWaiting { get; set; }

    public override string ToString()
    {
        if (!MessageWaitingIndicationActive)
        {
            return string.Format("CodingGroup: {0}" + Environment.NewLine +
                                 "SMSEncoding: {1}" + Environment.NewLine +
                                 "MessageClass: {2}" + Environment.NewLine +
                                 "TextIsCompressed: {3}" + Environment.NewLine +
                                 "MessageClassBitsSet: {4}", CodingGroup, SMSEncoding, MessageClass, TextIsCompressed, MessageClassBitsSet);
        }
        return string.Format("CodingGroup: {0}" + Environment.NewLine +
                             "SMSEncoding: {1}" + Environment.NewLine +
                             "MessageClass: {2}" + Environment.NewLine +
                             "TextIsCompressed: {3}" + Environment.NewLine +
                             "MessageClassBitsSet: {4}" + Environment.NewLine +
                             "MessageWaitingIndicationActive: {5}" + Environment.NewLine + 
                             "TypeOfMessageWaiting: {6}", CodingGroup, SMSEncoding, MessageClass, TextIsCompressed, MessageClassBitsSet, MessageWaitingIndicationActive, TypeOfMessageWaiting);
    }

}