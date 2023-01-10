using NLog;
using SMSTerminal.General;

namespace SMSTerminal.PDU
{
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
                        return PDUOctetHexToSeptet(pduUserDataHeader, message);
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

        private string PDUOctetHexToSeptet(PDUUserDataHeader pduUserDataHeader, string pduMessagePart)
        {
            //E8329BFD4697D9EC37
            //123456789012345678
            // 1 2 3 4 5 6 7 8 9
            //Length = 18
            var octetList = new List<string>();
            var septetList = new List<string>();

            var internalPDUMessagePart = pduMessagePart;
            if (pduUserDataHeader != null)
            {
                if (pduUserDataHeader.RequiresPadding(CodingDirection.Decoding))
                {
                    //Logger.Debug("Padding required before decoding. Adding {0} to the message to be padded.", pduUserdataHeader.PaddingAsHexString(CodingDirection.Decoding));
                    internalPDUMessagePart = pduUserDataHeader.PaddingAsHexString(CodingDirection.Decoding) + internalPDUMessagePart;
                    //Logger.Debug("Message to be decoded is now {0}", internalPDUMessagePart);
                }
            }

            //Converting from hex to octet
            for (var x = 0; x < internalPDUMessagePart.Length / 2; x++)
            {
                var hexChar1 = internalPDUMessagePart.Substring(x * 2, 1);
                var hexChar2 = internalPDUMessagePart.Substring((x * 2) + 1, 1);
                var binString1 = PDUFunctions.GetHexNibbleRepresentation(hexChar1);
                var binString2 = PDUFunctions.GetHexNibbleRepresentation(hexChar2);
                //Logger.Debug("[{0}] : {1}{2}b   {3}d", x, binString1, binString2, Convert.ToByte(binString1 + binString2, 2));
                octetList.Add(binString1 + binString2);
            }
            //Logger.Debug("OctetList.Count = {0}", octetList.Count);
            //Converting from octet to septet
            //-------------
            var listIndex = 0;
            var numberOfBitsToKeep = 7;
            //-----------
            while (listIndex < octetList.Count)
            {
                //Logger.Debug("ListIndex = {0}. Bits to keep {1}pcs).", listIndex, numberOfBitsToKeep);
                var firstOctet = listIndex == 0;
                var lastOctet = listIndex == octetList.Count - 1;
                var currentOctet = octetList[listIndex];

                string previousOctet = firstOctet ? "0000000" : octetList[listIndex - 1];
                //Logger.Debug("Current octet[{0}] = {1} byte({2}), previousOctet[{3}] = {4} byte({5})", listIndex, currentOctet, Convert.ToByte(currentOctet, 2), (listIndex - 1), previousOctet, Convert.ToByte(previousOctet, 2));

                if (numberOfBitsToKeep == 7)
                {
                    if (!firstOctet)
                    {
                        var freshSeptet1 = previousOctet[..7];
                        //Logger.Debug("Adding new septet by copying 7 bits from previous octet. Octet {0} -> septet -> {1} byte(*{2}*)", previousOctet, freshSeptet1, Convert.ToByte(freshSeptet1, 2));
                        //Logger.Debug("*** Adding {0} ***", Convert.ToByte(freshSeptet1, 2));
                        septetList.Add(freshSeptet1);
                    }
                    var freshSeptet2 = PDUFunctions.ReverseString(PDUFunctions.ReverseString(currentOctet)[..numberOfBitsToKeep]);
                    //Logger.Debug("Keeping 7 bits, adding none. Octet {0} truncated into septet -> {1} byte(*{2}*)", currentOctet, freshSeptet2, Convert.ToByte(freshSeptet2, 2));
                    //Logger.Debug("*** Adding {0} ***", Convert.ToByte(freshSeptet2, 2));
                    septetList.Add(freshSeptet2);
                    numberOfBitsToKeep--;
                }
                else
                {
                    var bitsToKeep = PDUFunctions.ReverseString(PDUFunctions.ReverseString(currentOctet)[..numberOfBitsToKeep]);
                    var bitsToGet = previousOctet[..(7 - numberOfBitsToKeep)];
                    var freshSeptet = bitsToKeep + bitsToGet;
                    //Logger.Debug("Fetching \"{0}\" from previous octet {1} and adding them to {2} = {3} byte(*{4}*) which was originally {5}", bitsToGet, previousOctet, bitsToKeep, freshSeptet, Convert.ToByte(freshSeptet, 2), currentOctet);
                    //Logger.Debug("*** Adding {0} ***", Convert.ToByte(freshSeptet, 2));
                    septetList.Add(freshSeptet);
                    if (lastOctet && numberOfBitsToKeep == 1)
                    {
                        var freshSeptet1 = currentOctet[..7];
                        if (freshSeptet1 != "0000000")
                        {
                            //Logger.Debug("[Special] Adding a last (& new) septet by copying 7 bits from current octet. Octet {0} -> septet -> {1} byte(*{2}*)", currentOctet, freshSeptet1, Convert.ToByte(freshSeptet1, 2));
                            //Logger.Debug("*** Adding {0} ***", Convert.ToByte(freshSeptet1, 2));
                            septetList.Add(freshSeptet1);
                        }
                    }
                    if (numberOfBitsToKeep == 1)
                    {
                        numberOfBitsToKeep = 7;
                    }
                    else
                    {
                        numberOfBitsToKeep--;
                    }
                }
                listIndex += 1;
                //Logger.Debug("-----------------------------------------------------");
                //break;
            }

            var byteArray = new byte[septetList.Count];

            for (var i = 0; i < septetList.Count; i++)
            {
                //Logger.Debug("[{0}] : {1}b   {2}d", i, septetList[i], Convert.ToByte(septetList[i], 2));
                byteArray[i] = PDUFunctions.FromSevenBits(septetList[i]);
            }
            //Logger.Debug("SeptetList.Count = {0}", septetList.Count);
            //Logger.Debug("Bytearray length = {0}", byteArray.Length);
            var gsmCharSet0338 = new GsmCharSet0338();
            var result = gsmCharSet0338.GetString(byteArray);
            if (pduUserDataHeader != null)
            {
                //Remove leading @ stemming from the padding.
                result = result[(pduUserDataHeader.LengthInOctets() + 1)..];
            }
            //Logger.Debug(result);
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
}
