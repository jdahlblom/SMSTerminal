using NLog;
using SMSTerminal.General;

namespace SMSTerminal.PDU;

/// <summary>
/// Specifies how the message should be formatted and processed. Is not obligatory.
/// </summary>
public class PDUUserDataHeader
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    /*
     * GSM 3.40 9.2.3.24
     * 
     * Supports only Concatenated SMS & headers containing 1 Information Element.
     * 
     */
    private readonly List<PDUInformationElement> _informationElementList = new();

    /*
     * 
     * 
     */
    private readonly string _userDataHeaderAndData = ""; //This includes the optional UDH and the actual message
    private Exception _lastParseException = null;
    private bool _headerHasBeenParsed = true;

    public PDUUserDataHeader(){}

    public PDUUserDataHeader(string userDataHeaderAndData)
    {
        _headerHasBeenParsed = false;
        _userDataHeaderAndData = userDataHeaderAndData;
        //0003420301
    }

    public bool ParseUserDataHeader()
    {
        var result = true;
        _headerHasBeenParsed = true;
        _lastParseException = null;
        try
        {

            if (string.IsNullOrEmpty(_userDataHeaderAndData) || _userDataHeaderAndData.Length < 6)
            {
                throw new ArgumentException($"[PDUUserDataHeader] Cannot parse null or empty UserDataHeader or data where length is less than 3 octets. ->{_userDataHeaderAndData}<-");
            }
            var userDataHeaderLength = Convert.ToByte(_userDataHeaderAndData[..2], 16);
            var userDataHeader = _userDataHeaderAndData.Substring(2, userDataHeaderLength * 2);
            MessagePartUndecoded = _userDataHeaderAndData[(userDataHeaderLength * 2 + 2)..];
            while(userDataHeader.Length > 0)
            {
                var informationElementIdentifier = (IEIEnum) Convert.ToByte(userDataHeader[..2], 16);
                switch (informationElementIdentifier)
                {
                    case IEIEnum.Concatenated_Short_Messages_8Bit_Reference:
                    case IEIEnum.Concatenated_Short_Messages_16Bit_Reference:
                    {
                        var tempInformationElement = PDUIEICSMS.ParseUserDataHeaderHexString(ref userDataHeader);
                        InformationElementList.Add(tempInformationElement);
                        break;
                    }
                    default:
                    {
                        var tempInformationElement = PDUIEINotSupported.ParseUserDataHeaderHexString(ref userDataHeader);
                        InformationElementList.Add(tempInformationElement);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _lastParseException = ex;
            result = false;
        }
        return result;
    }

    public int LengthInOctets()
    {
        var header = GetHeaderAsHexString();
        if(string.IsNullOrEmpty(header))
        {
            return 0;
        }
        //Logger.Debug("[PDUUserDataHeader] [{0}] Length of udh (no padding) is {1}" , header, header.Length / 2);
        return header.Length / 2;
    }

    public string Padding(CodingDirection codingDirection)
    {
        var result = "";
        var length = LengthInOctets();
        if (codingDirection == CodingDirection.Encoding)
        {
            /*
            * WHEN ENCODING
            * Remember this! The padding consisting of a series of '@' is also subjected to the transformation of octets to septets.
            * Transforming octets to septets means that every 7:th octet will be compressed -> "disappear".
            * To compensate for this this property must return 1 extra '@' for each 7 bytes the UDH is.
            */
            var extraPaddingThatWillDisappear = length/7;
            result = result.PadRight(length + extraPaddingThatWillDisappear, '@');
        }
        result = result.PadRight(length, '@');
        return result;
    }

    public string PaddingAsHexString(CodingDirection codingDirection)
    {
        return "".PadRight(Padding(codingDirection).Length*2, '0');
    }

    public bool RequiresPadding(CodingDirection codingDirection)
    {
        return !string.IsNullOrEmpty(PaddingAsHexString(codingDirection));
    }

    public void Add(PDUInformationElement pduInformationElement)
    {
        _informationElementList.Add(pduInformationElement);
    }

    public bool ContainsNotSupportedInformationElements()
    {
        return !ContainsOnlySupportedInformationElements();
    }

    public bool ContainsOnlySupportedInformationElements()
    {
        if(_informationElementList == null)
        {
            return true;
        }
        foreach (var pduInformationElement in _informationElementList)
        {
            if (Enum.GetName(typeof (IEIEnum), pduInformationElement.IEI).StartsWith("NS_"))
            {
                return false;
            }
        }
        return true;
    }

    public string GetHeaderAsHexString()
    {
        if(!_headerHasBeenParsed)
        {
            throw new InvalidOperationException("User Data Header has not been parsed.");
        }
        var byteList = new List<byte>();
        foreach (var pduInformationElement in InformationElementList)
        {
            switch (pduInformationElement.IEI)
            {
                case IEIEnum.Concatenated_Short_Messages_8Bit_Reference :
                case IEIEnum.Concatenated_Short_Messages_16Bit_Reference:
                {
                    byteList.Add(pduInformationElement.GetBytes());
                    break;
                }
            }
        }
        //Insert length of UDH
        byteList.Insert(0, (byte) byteList.Count);
            
        return byteList.ToHexString();
    }

    public string MessagePartUndecoded { get; set; } = "";

    public bool HasParseException => _lastParseException != null;

    public Exception LastParseException => _lastParseException;

    public bool ContainsConcatenationInformationElement
    {
        get
        {
            if(InformationElementList == null)
            {
                return false;
            }
            foreach (var pduInformationElement in InformationElementList)
            {
                if (pduInformationElement.IEI == IEIEnum.Concatenated_Short_Messages_8Bit_Reference || pduInformationElement.IEI == IEIEnum.Concatenated_Short_Messages_16Bit_Reference)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public List<PDUInformationElement> InformationElementList => _informationElementList;

    public override string ToString()
    {
        return string.Format(Environment.NewLine + "PDUUserDataHeader : " + Environment.NewLine + "------------------------" + Environment.NewLine +
                             "UserDataHeader Length: {0}" + Environment.NewLine +
                             "InformationElements: {1}" + Environment.NewLine +
                             "PDU: {2}" + Environment.NewLine +
                             "RawMessage: {3}" + Environment.NewLine + 
                             "------------------------", LengthInOctets(), InformationElementList.ElementsToString(), _userDataHeaderAndData, MessagePartUndecoded);
    }
        
}