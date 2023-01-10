using System.Collections;
using NLog;
using SMSTerminal.General;

//@£$¥èéùìòÇØøÅå?_ΦΓΛΩΠΨΣΘΞ?ÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà^{}\\[~]|€@£$¥èéùìòÇØøÅå?_ΦΓΛΩΠΨΣΘΞ?ÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà^{}\\[~]|€
//
namespace SMSTerminal.PDU;

/// <summary>
/// Encodes outgoing message into PDU using chosen SMS encoding.
/// </summary>
public class PDUEncoder
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private string _phoneNumber;
    private string[] _concatenatedMessages;

    public PDUEncoder(string phoneNumber, string message, SMSEncoding smsEncoding, int daysValid, bool requestStatusReport = false, bool rejectDuplicates = true)
    {
        PDUHeaderObject = new PDUHeader(MessageDirection.OUTGOING);
        PhoneNumber = phoneNumber;
        RequestStatusReport = requestStatusReport;
        RejectDuplicates = rejectDuplicates;
        Message = message;
        SmsEncoding = smsEncoding;
        SMSValidityPeriod = new PDUValidityPeriod(PDUHeaderObject, new TimeSpan(daysValid, 0, 0, 0));
    }

    public  string[] MakePDU()
    {
        return NeedsToBeSplit(Message, SmsEncoding) ? GetConcatenatedMessagePDU() : GetSingleMessagePDU();
    }


    /* ~ [PDU Header] + [PDU PARAMS] + [ENCODED MESSAGE] */
    private string[] GetSingleMessagePDU()
    {
        if (string.IsNullOrEmpty(Message))
        {
            throw new ArgumentException("[PDUEncoder] Message cannot be null or empty");
        }
        if (NeedsToBeSplit(Message, SmsEncoding))
        {
            throw new ArgumentException($"Single SMS Message Length Invalid ({(string.IsNullOrEmpty(Message) ? "null" : Message.Length.ToString())}) " +
                                        $"Max message length in characters: 7-bit 160, 8-bit 140, UTF-16 70. This message was {Message.Length} characters long and encoding set to {SmsEncoding}");
        }
            
        /*
         * PDU Octet
         */
        PDUHeaderObject.SmsMessageType = SMSMessageType.SMS_SUBMIT;
        PDUHeaderObject.ValidityPeriodFormatUsed = ValidityPeriodFormat.Relative;
        PDUHeaderObject.RejectDuplicates = RejectDuplicates;
        PDUHeaderObject.StatusReportRequested = RequestStatusReport;
        /*
         * 
         */

        return new[] { GetPDU(null, Message) };
    }

    /* ~ [PDU Header] + [PDU PARAMS] + [USER DATA HEADER] + [ENCODED MESSAGE] */

    private  string[] GetConcatenatedMessagePDU()
    {
        if (string.IsNullOrEmpty(Message))
        {
            throw new ArgumentException("[PDUEncoder] Message cannot be null or empty");
        }

        var result = new ArrayList();

        switch (SmsEncoding)
        {
            case SMSEncoding._7bit:
            {
                SplitMessageBits7();
                break;
            }
            case SMSEncoding._8bit:
            {
                SplitMessageBits8();
                break;
            }
            case SMSEncoding._UCS2:
            {
                SplitMessageUCS2();
                break;
            }
            case SMSEncoding.ReservedMask:
            default:
            {
                throw new Exception($"Unsupported encoding chosen : {SmsEncoding}.");
            }
        }
        /*
         * PDU Header (same in all parts of the concat message)
         */
        PDUHeaderObject.UserDataHeaderExists = true;
        PDUHeaderObject.SmsMessageType = SMSMessageType.SMS_SUBMIT;
        PDUHeaderObject.ValidityPeriodFormatUsed = ValidityPeriodFormat.Relative;
        PDUHeaderObject.RejectDuplicates = RejectDuplicates;
        PDUHeaderObject.StatusReportRequested = RequestStatusReport;
        /*
         * UserDataHeader & message different in each part of the concat message
         */
        var csmsMessageReference = PDUIEICSMS.GenerateCSMSMessageReference();
        for (var i = 0; i < _concatenatedMessages.Length; i++)
        {
            var userDataHeader = new PDUUserDataHeader();
            var pduCSms = new PDUIEICSMS
            {
                IEI = IEIEnum.Concatenated_Short_Messages_16Bit_Reference,
                MessagePartsTotal = _concatenatedMessages.Length,
                MessageReference = csmsMessageReference,
                ThisPart = i + 1
            };
            userDataHeader.Add(pduCSms);
            UserDataHeaderObject = userDataHeader;
            result.Add(GetPDU(userDataHeader, _concatenatedMessages[i]));
        }
        return result.ToArray(typeof(string)) as string[];
    }
        
    private string GetPDU(PDUUserDataHeader userDataHeader, string message)
    {
        //Logger.Debug("SMSC Length = ->00<-");
        var encodedData = "00"; // Zero length -> Use SMSC found in phone.
        /*
         * 
         */
        var pduTypeHex = Convert.ToString(PDUHeaderObject.Octet(), 16).PadLeft(2, '0'); //First octet describes the PDU
        //Logger.Debug("First Octet = ->{0}<-->{1}<-", pduTypeHex.ToUpper(), _pduHeaderObject);
        encodedData += pduTypeHex;
        /*
         * 
         */
        var messageReference = Convert.ToString(MessageReference, 16).PadLeft(2, '0');
        //Logger.Debug("TP Message reference = ->{0}<- [00] -> Device sets (increments) the reference itself.", messageReference.ToUpper());
        encodedData += messageReference;
        /*
         * 
         */
        var encodedTph = EncodePhoneNumber(PhoneNumber);
        //Logger.Debug("[Tph Length] & [Type Of Address 0x81/0x91] & [Tph] = ->{0}<-", encodedTph.ToUpper());
        encodedData += encodedTph;
        /*
         * Outgoing SMS from MS does not need to set this(?) 13.04.2011 
         */
        //Logger.Debug("TP_PID Protocol Identifier (SMS) = ->00<-  Note that for the straightforward case of simple MS-to-SC short message transfer the Protocol Identifier is set to the value 0.");
        encodedData += "00"; //Protocol identifier (Short Message Type 0x0, Status Report 0x1)
        /*
         * 
         */
        var encodingScheme = Convert.ToString((int)SmsEncoding, 16).PadLeft(2, '0');
        //Logger.Debug("TP-DCS Data coding scheme = ->{0}<-", encodingScheme.ToUpper());
        encodedData += encodingScheme;
        /*
         * 
         */
        if (PDUHeaderObject.ValidityPeriodFormatUsed != ValidityPeriodFormat.FieldNotPresent)
        {
            var validityPeriod = Convert.ToString(SMSValidityPeriod.ValidityPeriodInt, 16).PadLeft(2, '0');
            //Logger.Debug("TP-Validity-Period = ->{0}<-", validityPeriod.ToUpper());
            encodedData += validityPeriod; //Validity Period
        }

        switch (SmsEncoding)
        {
            case SMSEncoding._7bit:
            {
                var messageLengthInt = CalculateMessageLength7Bit(userDataHeader, message);
                var messageLength = Convert.ToString(messageLengthInt, 16).PadLeft(2, '0');
                //Logger.Debug("[7-bit] TP-User-Data-Length (msg len) = ->{0}<- decimal = {1}", messageLength.ToUpper(), messageLengthInt);
                encodedData += messageLength;
                var encodedMessage = StringToSeptetToOctetHex(userDataHeader, message);
                //Logger.Debug("[7-bit] TP-User-Data = ->{0}<-", encodedMessage.ToUpper());
                encodedData += encodedMessage;
                break;
            }
            case SMSEncoding._8bit:
            {
                var messageLengthInt = CalculateMessageLength8Bit(userDataHeader, message);
                var messageLength = Convert.ToString(messageLengthInt, 16).PadLeft(2, '0');
                //Logger.Debug("[8-bit] TP-User-Data-Length (msg len) = ->{0}<- decimal = {1}", messageLength.ToUpper(), messageLengthInt);
                encodedData += messageLength;
                var encodedMessage = StringToEightBitHex(userDataHeader, message);
                //Logger.Debug("[8-bit] TP-User-Data = ->{0}<-", encodedMessage.ToUpper());
                encodedData += encodedMessage;
                break;
            }
            case SMSEncoding._UCS2:
            {
                var messageBytes = PDUFunctions.EncodeUCS2(message);
                var messageLengthInt = CalculateMessageLengthUCS2(userDataHeader, messageBytes);
                var messageLength = Convert.ToString(messageLengthInt, 16).PadLeft(2, '0');
                //Logger.Debug("[UCS2] TP-User-Data-Length (msg len) = ->{0}<- decimal = {1} Big Endian byte count (msg only) = {2} bytes",messageLength.ToUpper(), messageLengthInt, messageBytes.Length);
                encodedData += messageLength; //Length of message

                if (userDataHeader != null)
                {
                    encodedData += userDataHeader.GetHeaderAsHexString();
                }

                foreach (var b in messageBytes)
                {
                    var str = Convert.ToString(b, 16).PadLeft(2, '0');
                    encodedData += str;
                }
                break;
            }
            case SMSEncoding.ReservedMask:
            default:
                throw new Exception("Exception switching SMSEncoding.");
        }
        //Logger.Debug("Final result = ->{0}<-", encodedData.ToUpper());
        return encodedData.ToUpper();
    }

    private  bool NeedsToBeSplit(string message, SMSEncoding smsEncoding)
    {
        return smsEncoding switch
        {
            SMSEncoding._7bit => message.Length > PDUFunctions.Max7BitSingle,
            SMSEncoding._8bit => message.Length > PDUFunctions.Max8BitSingle,
            SMSEncoding._UCS2 => message.Length > PDUFunctions.MaxUtf16Single,
            _ => throw new Exception($"SMS Encoding not supported. {smsEncoding}")
        };
    }

    private void SplitMessageBits7()
    {
        var originalMessage = Message;
        var result = new ArrayList();
        var gsmCharSet = new GsmCharSet0338();
        var bytesRead = 0;
        var portion = "";
        while (originalMessage.Length > 0)
        {
            if (gsmCharSet.IsExtended(originalMessage[..1]))
            {
                //Requires 2 bytes!
                if (bytesRead + 2 > PDUFunctions.Max7BitConcatenated)
                {
                    //Switch to new portion, current will overflow
                    result.Add(portion);
                    bytesRead = 0;
                    portion = "";
                }
                bytesRead += 2;
                portion += originalMessage[..1];
            }
            else
            {
                //Requires 1 byte!
                if (bytesRead + 1 > PDUFunctions.Max7BitConcatenated)
                {
                    //Switch to new portion, current will overflow
                    result.Add(portion);
                    bytesRead = 0;
                    portion = "";
                }
                bytesRead++;
                portion += originalMessage[..1];
            }
            originalMessage = originalMessage.Remove(0, 1);
            //Logger.Debug("[7-bit] Remaining message is {0} long Current portion is {1} long", originalMessage.Length, portion.Length);
        }
        result.Add(portion);
        _concatenatedMessages = result.ToArray(typeof(string)) as string[];
        //Logger.Debug("[7-bit] Message split into {0} parts", (_concatenatedMessages != null ? _concatenatedMessages.Length : 0));
        if (_concatenatedMessages == null)
        {
            return;
        }
        /*foreach (var concatenatedMessage in _concatenatedMessages)
        {
            Logger.Debug("[7-bit] Message[] (length={0}) {1}", concatenatedMessage.Length, concatenatedMessage);
        }*/
    }

    private void SplitMessageBits8()
    {
        var originalMessage = Message;
        var result = new ArrayList();
        var bytesRead = 0;
        var portion = "";
        while (originalMessage.Length > 0)
        {
            //Requires 1 byte!
            if (bytesRead + 1 > PDUFunctions.Max8BitConcatenated16BitRef)
            {
                //Switch to new portion, current will overflow
                result.Add(portion);
                bytesRead = 0;
                portion = "";
            }
            bytesRead += 1;
            portion += originalMessage[..1];

            originalMessage = originalMessage.Remove(0, 1);
            //Logger.Debug("[8-bit] Remaining message is {0} long. Current portion is {1} long", originalMessage.Length, portion.Length);
        }
        result.Add(portion);
        _concatenatedMessages = result.ToArray(typeof(string)) as string[];
        //Logger.Debug("[8-bit] Message split into {0} parts", (_concatenatedMessages != null ? _concatenatedMessages.Length : 0));
        if (_concatenatedMessages == null)
        {
            return;
        }
        foreach (var concatenatedMessage in _concatenatedMessages)
        {
            //Logger.Debug("[8-bit] Message[] (length={0}) {1}", concatenatedMessage.Length, concatenatedMessage);
        }
    }

    private void SplitMessageUCS2()
    {
        var originalMessage = Message;
        var result = new ArrayList();
        var charsRead = 0;
        var portion = "";
        while (originalMessage.Length > 0)
        {
            //UCS2 Requires 2 bytes per character!
            if (charsRead + 1 > PDUFunctions.MaxUtf16Concatenated16BitRef)
            {
                //Switch to new portion, current will overflow
                result.Add(portion);
                charsRead = 0;
                portion = "";
            }
            charsRead += 1;
            portion += originalMessage[..1];
            originalMessage = originalMessage.Remove(0, 1);
            //Logger.Debug("[UCS2] Remaining message is {0} long. Current portion is {1} long", originalMessage.Length, portion.Length);
        }
        result.Add(portion);
        _concatenatedMessages = result.ToArray(typeof(string)) as string[];
        //Logger.Debug("[UCS2] Message split into {0} parts", (_concatenatedMessages != null ? _concatenatedMessages.Length : 0));
        if (_concatenatedMessages == null)
        {
            return;
        }
        foreach (var concatenatedMessage in _concatenatedMessages)
        {
            //Logger.Debug("[UCS2] Message[] (length={0}) {1}", concatenatedMessage.Length, concatenatedMessage);
        }
    }

    private  int CalculateMessageLength7Bit(PDUUserDataHeader userDataHeader, string message)
    {
        /*
         * *******************************************************************************************
         * This is important!! The length must be the SEPTET LENGTH(!) of the message or UDH + message
         * *******************************************************************************************
         */
        var septetList = StringToSeptetList(userDataHeader, message);
        if (userDataHeader != null)
        {
            //Logger.Debug("[7-bit] Message length (attn: in SEPTETS!) [UDH]+[Message] is decimal {0} [UDH] = {1}", septetList.Count, userDataHeader.GetHeaderAsHexString());
        }
        else
        {
            //Logger.Debug("[7-bit] Message length is decimal {0}, no [UDH] exists", septetList.Count);
        }
        return septetList.Count;
    }

    private  int CalculateMessageLength8Bit(PDUUserDataHeader userDataHeader, string str)
    {
        var result = str.Length;
        if (userDataHeader != null)
        {
            //Logger.Debug("[8-bit] Message length [UDH]+[Message] is decimal {0} {1} = {2} [UDH] = {3}", result,userDataHeader.LengthInOctets(), (result + userDataHeader.LengthInOctets()), userDataHeader.GetHeaderAsHexString());
            result += userDataHeader.LengthInOctets();
        }
        else
        {
            //Logger.Debug("[8-bit] Message length is decimal {0}, no [UDH] exists", result);
        }
        return result;
    }

    private  int CalculateMessageLengthUCS2(PDUUserDataHeader userDataHeader, byte[] bytes)
    {
        var result = bytes.Length;
        if (userDataHeader != null)
        {
            //Logger.Debug("[UCS2] Message length [UDH]+[Message] is decimal {0} {1} = {2} [UDH] = {3}",result,userDataHeader.LengthInOctets(),(result + userDataHeader.LengthInOctets()),userDataHeader.GetHeaderAsHexString());
            result += userDataHeader.LengthInOctets();
        }
        else
        {
            //Logger.Debug("[UCS2] Message length is decimal {0}, no [UDH] exists", result);
        }
        return result;
    }

    private  string EncodePhoneNumber(string phoneNumber)
    {
        var isInternational = phoneNumber.StartsWith("+");
        if (isInternational)
        {
            phoneNumber = phoneNumber.Remove(0, 1);
        }

        var lengthOctet = Convert.ToString(phoneNumber.Length, 16).PadLeft(2, '0').ToUpper();
        var typeOfAddressOctet = isInternational ? "91" : "81";

        //var header = (phoneNumber.Length << 8) + 0x81 | (isInternational ? 0x10 : 0x20);
        var header = lengthOctet + typeOfAddressOctet;

        //Logger.Debug("Length octet for Tph = {0} -> Tph number length is {1}. Tph header = ->{2}<-",lengthOctet,phoneNumber.Length,header);

        if (phoneNumber.Length % 2 == 1)
        {
            phoneNumber = phoneNumber.PadRight(phoneNumber.Length + 1, 'F');
        }
        phoneNumber = PDUFunctions.SwapNibbles(phoneNumber);
        //Logger.Debug("Tph (swapped nibbles)= ->{0}<-", phoneNumber);

        var result = header + phoneNumber;
        //Logger.Debug("Tph header [len] [type] [tph] = ->{0}<-", result);
        return result;
    }

    private  string StringToSeptetToOctetHex(PDUUserDataHeader userDataHeader, string message)
    {
        var septetList = StringToSeptetList(userDataHeader, message);
        return SeptetToOctetHex(userDataHeader, septetList);
    }

    private  List<string> StringToSeptetList(PDUUserDataHeader userDataHeader, string message)
    {
        if (userDataHeader != null)
        {
            //Logger.Debug("[7-bit] UserDataHeader will be used.");
            //These will be "septeted" resulting in 'n' zeroes (GSM 7-bit alphabet)
            //The UDH is length * 8 bits long,  the messages starts at a septet boundary, thus the udh must be 7-bit word aligned.
            //'n' first @'s will be overwritten by the UDH leaving the rest (1-6) to be translated to zeroes.
            message = message.Insert(0, userDataHeader.Padding(CodingDirection.Encoding));
        }
        var gsmCharSet0338 = new GsmCharSet0338();
        var septetList = new List<string>();
        var escByteString = PDUFunctions.ToSevenBits(27);

        var messageLength = message.Length;
        for (var i = 0; i < messageLength; i++)
        {
            var s = message[i];

            var b = gsmCharSet0338.GetByte(s);
            if (gsmCharSet0338.IsExtended(s))
            {
                //Add escape char before adding extended character
                septetList.Add(escByteString);
                ////Logger.Debug("[7-bit] SeptetList[{0}] {1} byte(27) GSM 03.38", (septetList.Count - 1), escByteString);
            }
            var septetString = PDUFunctions.ToSevenBits(b);
            septetList.Add(septetString);
            ////Logger.Debug("[7-bit] SeptetList[{0}] {1} byte({2}) GSM 03.38", (septetList.Count - 1), septetString, b);
        }
        return septetList;
    }

    private  string SeptetToOctetHex(PDUUserDataHeader userDataHeader, List<string> septetList)
    {
        var octetList = new List<string>();
        //Converting from septet to octet
        //-------------
        var numberOfBitsToAdd = 1;
        var currentIndex = 0;
        //-----------
        while (currentIndex < septetList.Count)
        {
            var isLastSeptet = currentIndex == septetList.Count - 1;
            var currentSeptet = septetList[currentIndex];
            var nextSeptet = isLastSeptet ? "0000000" : septetList[currentIndex + 1];
            //-------------- Swapping----------
            var bitsToAddInWrongOrder = PDUFunctions.ReverseString(nextSeptet)[..numberOfBitsToAdd];
            //----------Reverse Swapping-------
            var bitsToAdd = PDUFunctions.ReverseString(bitsToAddInWrongOrder);

            //Discard the rightmost bits (>8), they have already been added and should be discarded
            var freshOctet = (bitsToAdd + currentSeptet)[..8];
            /*Logger.Debug("[7-bit] Taking LeftMost Bits->[{0}] from NextSeptet[{1}] and prepending to Current Septet[{2}] ----> (octet) {3}",
                bitsToAdd,
                nextSeptet,
                currentSeptet,
                freshOctet);*/

            octetList.Add(freshOctet);
            numberOfBitsToAdd++;
            if (numberOfBitsToAdd == 8)
            {
                numberOfBitsToAdd = 1;
                currentIndex += 1;
            }
            currentIndex += 1;
        }
        var result = "";

        //Converting from octet to hex
        foreach (var oct in octetList)
        {
            var firstHalf = oct[..4];
            var secondHalf = oct.Substring(4, 4);
            var hex1 = PDUFunctions.GetNibbleHexRepresentation(firstHalf);
            var hex2 = PDUFunctions.GetNibbleHexRepresentation(secondHalf);
            //Logger.Debug("[7-bit] Octet {0} equals Hex {1}",  oct, hex1 + hex2);
            result += hex1 + hex2;
        }
        if (userDataHeader != null)
        {
            //Logger.Debug("[7-bit] Inserting PDUUserDataHeader ->{0}<- {1}", userDataHeader.GetHeaderAsHexString(), userDataHeader);
            //Logger.Debug("[7-bit] Message before UDH\n->{0}<-", result);
            result = userDataHeader.GetHeaderAsHexString() + result[(userDataHeader.LengthInOctets() * 2)..];
            //Logger.Debug("[7-bit] Message after UDH\n->{0}<-", result);
        }
        return result;
    }

    private  string StringToEightBitHex(PDUUserDataHeader userDataHeader, string message)
    {
        var result = "";

        //Logger.Debug("[8-bit] UserDataHeader will be used = {0}.", userDataHeader != null);

        var octetList = new List<string>();

        foreach (var t in message)
        {
            var b = (byte)t;
            var octetString = PDUFunctions.ToEightBits(b);
            octetList.Add(octetString);
            //Logger.Debug("[8-bit] OctetList[{0}] {1} byte({2})", (octetList.Count - 1), octetString, b);
        }

        //Converting from octet to hex
        foreach (var oct in octetList)
        {
            var firstHalf = oct[..4];
            var secondHalf = oct.Substring(4, 4);
            var hex1 = PDUFunctions.GetNibbleHexRepresentation(firstHalf);
            var hex2 = PDUFunctions.GetNibbleHexRepresentation(secondHalf);
            //Logger.Debug("[8-bit] Octet[{0}] {1} equals Hex {2}", x, oct, hex1 + hex2);
            result += hex1 + hex2;
        }

        if (userDataHeader != null)
        {
            //Logger.Debug("[8-bit] Inserting PDUUserDataHeader ->{0}<-", userDataHeader.GetHeaderAsHexString());
            //Logger.Debug("[8-bit] Message before UDH->{0}<-", result);
            result = userDataHeader.GetHeaderAsHexString() + result;
            //Logger.Debug("[8-bit] Message after UDH->{0}<-", result);
        }
        return result;
    }

    private bool RequestStatusReport { get; set; }
    private bool RejectDuplicates { get; set; }
    private byte MessageReference { get; set; }
    private string PhoneNumber
    {
        get => _phoneNumber;
        set => _phoneNumber = value.Replace(" ", "").Replace("-", "");
    }
    private string Message { get; set; }
    private bool MoreMessagesToSend { get; set; }
    private string ServiceCenterNumber { get; set; }
    private DateTime ServiceCenterTimeStamp { get; set; }
    private PDUUserDataHeader UserDataHeaderObject { get; set; }
    private PDUHeader PDUHeaderObject { get; set; }
    private SMSEncoding SmsEncoding { get; set; } = SMSEncoding._7bit;
    private PDUValidityPeriod SMSValidityPeriod { get; set; }

}