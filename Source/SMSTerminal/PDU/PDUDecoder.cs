using NLog;
using SMSTerminal.General;

namespace SMSTerminal.PDU;

/// <summary>
/// Decodes PDU into readable format depending on the SMS encoding and User Data Header used.
/// </summary>
public class PDUDecoder
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    //ONLY MESSAGE PART NO USER DATA HEADER!!!
    public string Decode(PDUUserDataHeader pduUserDataHeader, SMSEncoding smsEncoding, string message)
    {
        switch (smsEncoding)
        {
            case SMSEncoding._7bit:
                {
                    return PDUDecode7Bit(pduUserDataHeader, message);
                }
            case SMSEncoding._8bit:
                {
                    return PDUFunctions.Decode8BitHex(message);
                }
            case SMSEncoding._UCS2:
                {
                    return PDUFunctions.DecodeUCS2FromHex(message);
                }
            case SMSEncoding.ReservedMask:
            default:
                {
                    throw new NotImplementedException($"Failed to determine encoding. PDUDecoder.Decode => {smsEncoding}");
                }
        }
    }

    private string PDUDecode7Bit(PDUUserDataHeader pduUserDataHeader, string pduMessagePart)
    {
        var internalPDUMessagePart = pduMessagePart;
        if (pduUserDataHeader != null)
        {
            if (pduUserDataHeader.RequiresPadding(CodingDirection.Decoding))
            {
                //Padding required before decoding
                internalPDUMessagePart = pduUserDataHeader.PaddingAsHexString(CodingDirection.Decoding) + internalPDUMessagePart;
            }
        }

        var bytes = new byte[internalPDUMessagePart.Length / 2];
        var arrayIndex = 0;
        //Converting from hex to octet
        for (var x = 0; x < internalPDUMessagePart.Length / 2; x++)
        {
            var hexChar1 = internalPDUMessagePart.Substring(x * 2, 1);
            var hexChar2 = internalPDUMessagePart.Substring(x * 2 + 1, 1);
            bytes[arrayIndex++] = Convert.ToByte(hexChar1 + hexChar2, 16);
        }

        //-------------
        var listIndex = 0;
        var numberOfBitsToKeep = 7;
        //-----------

        var resultList = new List<byte>();

        while (listIndex < bytes.Length)
        {
            var firstOctet = listIndex == 0;
            var lastOctet = listIndex == bytes.Length - 1;
            var currentOctet = bytes[listIndex];

            var previousOctet = (byte)(firstOctet ? 0 : bytes[listIndex - 1]);

            if (numberOfBitsToKeep == 7)
            {
                if (!firstOctet)
                {
                    //Adding new septet by copying 7 bits from previous octet.
                    resultList.Add((byte)(previousOctet >> 1));
                }
                //Keeping 7 bits, adding none.
                var newOctet = (byte)(currentOctet << (8 - numberOfBitsToKeep));
                newOctet = (byte)(newOctet >> (8 - numberOfBitsToKeep));
                resultList.Add(newOctet);

                numberOfBitsToKeep--;
            }
            else
            {
                //Set higher bit to zero
                var bitsToKeep = (byte)(currentOctet << 8 - numberOfBitsToKeep);
                bitsToKeep = (byte)(bitsToKeep >> 8 - numberOfBitsToKeep);

                //Get most significant bits
                var bitsToGet = (byte)(previousOctet >> numberOfBitsToKeep + 1);

                // OR them together with currentOctet having most significant bits
                var freshSeptet = (byte)(bitsToKeep << 7 - numberOfBitsToKeep);
                freshSeptet = (byte)(freshSeptet | bitsToGet);

                resultList.Add(freshSeptet);


                if (lastOctet && numberOfBitsToKeep == 1)
                {
                    if (currentOctet > 0)
                    {
                        //[Special] Adding a last (& new) septet by copying 7 bits from current octet. 
                        resultList.Add((byte)(currentOctet >> numberOfBitsToKeep));
                    }
                }

                numberOfBitsToKeep = numberOfBitsToKeep == 1 ? 7 : numberOfBitsToKeep - 1;
            }

            listIndex += 1;
        }

        var gsmCharSet0338 = new GsmCharSet0338();
        var result = gsmCharSet0338.GetString(resultList.ToArray());
        if (pduUserDataHeader != null)
        {
            //Remove leading @ stemming from the padding.
            result = result[(pduUserDataHeader.LengthInOctets() + 1)..];
        }

        return result;
    }

    public string DecodePhoneNumber(string typeOfAddressOctetAndPhoneNumber)
    {
        return DecodePhoneNumber(typeOfAddressOctetAndPhoneNumber[..2], typeOfAddressOctetAndPhoneNumber[2..]);
    }


    private string DecodePhoneNumber(string typeOfAddressOctet, string phoneNumber)
    {
        var toInternational = typeOfAddressOctet == "91";

        phoneNumber = PDUFunctions.SwapNibbles(phoneNumber).Replace("F", "");
        if (toInternational)
        {
            phoneNumber = phoneNumber.Insert(0, "+");
        }
        //Logger.Debug("Tph = ->{0}<-", phoneNumber);
        return phoneNumber;
    }

}