using System.Text;
using System.Text.RegularExpressions;
using SMSTerminal.Events;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.General;

public static class ExtensionMethods
{


    public static bool ContainsError(this ModemResultEnum modemResultEnum)
    {
        switch (modemResultEnum)
        {
            case ModemResultEnum.CMEError:
            case ModemResultEnum.CMSError:
            case ModemResultEnum.Error:
            case ModemResultEnum.ParseFail:
            case ModemResultEnum.Critical:
            case ModemResultEnum.IOError:
            case ModemResultEnum.TimeOutError:
            case ModemResultEnum.UnknownModemData:
                return true;
            case ModemResultEnum.None:
            case ModemResultEnum.Ok:
                return false;
            default:
                {
                    throw new Exception($"ModemResultEnum : {modemResultEnum} isn't implemented.");
                }
        }
    }


    public static bool IsInt(this string s)
    {
        return !string.IsNullOrEmpty(s) && int.TryParse(s, out _);
    }

    public static bool IsValidPIN(this string pin)
    {
        if (string.IsNullOrEmpty(pin) || pin.Length < 4 || pin.Length > 8)
        {
            return false;
        }

        return int.TryParse(pin, out _);
    }

    public static string MakeValidTph(this string telephone)
    {
        return string.IsNullOrEmpty(telephone) ? null : telephone.Replace("-", "").Replace(" ", "");
    }

    public static bool IsValidTph(this string telephone)
    {
        if (string.IsNullOrEmpty(telephone) || telephone.Length < 1)
        {
            return false;
        }
        //So what the hell is a valid telephone number?
        //Have to relax this inspection because of e.g. messages from the operator +15400 and so on.
        //Sequence of numbers longer than 0 and shorter than 15
        var tmpTelephone = telephone.Replace("+", "").Replace(" ", "").Replace("-", "");

        if (string.IsNullOrEmpty(tmpTelephone) || tmpTelephone.Length < 1)
        {
            return false;
        }
        if (tmpTelephone.Length > 15) //E.164 standard -> max 15 chars in international format GSM telephone number (includes +)
        {
            return false;
        }
        try
        {
            var isNotNumber = new Regex("[^0-9]");
            if (isNotNumber.IsMatch(tmpTelephone))
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public static bool ContainsOutputEndMarker(this string message)
    {
        if (string.IsNullOrEmpty(message)) return false;

        return SmsFunctions.StatusReportCDSIsComplete(message, out _) || 
               ATMarkers.NewMessageMarkerList.Any(message.Contains) ||
               ATCommands.ContainsResultCode(message) ||
               message.Contains(ATMarkers.ReadyPrompt) ||
               message.Contains(ATMarkers.IncomingCall1)||
               message.Contains(ATMarkers.IncomingCall2) ;
    }

    public static string DecodeException(this Exception e)
    {
        if (e == null)
        {
            return null;
        }
        return e.Message + "\n" + e.StackTrace;
    }

    public static void TrimEntries(this string[] stringArray)
    {
        for (var i = 0; i < stringArray.Length; i++)
        {
            stringArray[i] = stringArray[i].Trim();
        }
    }

    public static string RemoveCommonName(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        if (value.StartsWith("CN="))
        {
            value = value[3..];
        }
        return value;
    }

    public static bool StringFound(this List<string> list, string value)
    {
        return list.Any(s => s.Equals(value));
    }

    public static void Add(this List<byte> list, byte[] bytes)
    {
        foreach (var b in bytes)
        {
            list.Add(b);
        }
    }

    public static long MilliSecsNowDTO(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public static string ToHexString(this List<byte> list)
    {
        var result = "";
        var byteArray = list.ToArray();
        foreach (var t in byteArray)
        {
            result += Convert.ToString(t, 16).PadLeft(2, '0');
        }
        return result.ToUpper();
    }

    public static byte[] ToArray(this List<byte> list)
    {
        var result = new byte[list.Count];
        var i = 0;
        foreach (var b in list)
        {
            result[i] = b;
            i++;
        }
        return result;
    }

    public static readonly DateTime NullDateTime = new(1753, 2, 2);

    public static string Ddmmyyyy(this DateTimeOffset datetimeOffset)
    {
        if (datetimeOffset == DateTimeOffset.MinValue || datetimeOffset == NullDateTime)
        {
            return null;
        }
        return datetimeOffset.ToString(SmsFunctions.DateFormat);
    }

    public static string Ddmmyyyy(this DateTime datetime)
    {
        if (datetime == DateTime.MinValue || datetime == NullDateTime)
        {
            return null;
        }
        return datetime.ToString(SmsFunctions.DateFormat);
    }

    public static string Ddmmyyyyhhmmss(this DateTime datetime, bool concatToday)
    {
        if (datetime == DateTime.MinValue || datetime == NullDateTime)
        {
            return null;
        }
        if (concatToday && datetime.ToString(SmsFunctions.DateFormat) == DateTime.Today.ToString(SmsFunctions.DateFormat))
        {
            return datetime.ToString(SmsFunctions.DateTimeFormatConcat);
        }
        return datetime.ToString(SmsFunctions.DateTimeFormat);
    }

    public static string Ddmmyyyyhhmmss(this DateTimeOffset datetimeOffset, bool concatToday)
    {
        if (datetimeOffset == DateTimeOffset.MinValue || datetimeOffset == NullDateTime)
        {
            return null;
        }
        if (concatToday && datetimeOffset.ToString(SmsFunctions.DateFormat) == DateTime.Today.ToString(SmsFunctions.DateFormat))
        {
            return datetimeOffset.ToString(SmsFunctions.DateTimeFormatConcat);
        }
        return datetimeOffset.ToString(SmsFunctions.DateTimeFormat);
    }

    public static DateTime Ddmmyyyyhhmmss(this string datetime)
    {
        return DateTime.ParseExact(datetime, SmsFunctions.DateTimeFormat, null);
    }

    public static DateTime Ddmmyyyy(this string datetime)
    {
        return DateTime.ParseExact(datetime, SmsFunctions.DateFormat, null);
    }

    public static int MaxMessageLengthFound(this List<OutgoingSms> outgoingSmsList)
    {
        var result = 0;
        foreach (var outgoingSms in outgoingSmsList)
        {
            if (outgoingSms.Message.Length > result)
            {
                result = outgoingSms.Message.Length;
            }
        }
        return result;
    }

    public static SmsDirection GetDirection(this List<IShortMessageService> smsList)
    {
        var outgoingFound = smsList.Any(sms => sms.Direction == SmsDirection.Outgoing);
        var incomingFound = smsList.Any(sms => sms.Direction == SmsDirection.Incoming);
        if (incomingFound && outgoingFound)
        {
            return SmsDirection.Both;
        }
        if (incomingFound)
        {
            return SmsDirection.Incoming;
        }
        if (outgoingFound)
        {
            return SmsDirection.Outgoing;
        }
        throw new Exception("SmsDirection error. SmsDirection Enum in smsList has value not recognized?");
    }

    public static string ElementsToString(this IEnumerable<PDUInformationElement> pduInformationElementList)
    {
        var result = new StringBuilder();
        foreach (var pduInformationElement in pduInformationElementList)
        {
            result.Append(Environment.NewLine + pduInformationElement + Environment.NewLine);
        }
        return result.ToString();
    }

    public static string RemoveAtLineEndings(this string s)
    {
        return s?.Replace("\r", "").Replace("\n", "").Trim();
    }

    public static string GetTextForPrint(this IEnumerable<IShortMessageService> smsList)
    {
        var result = new StringBuilder();
        foreach (var sms in smsList)
        {
            if (result.Length > 0)
            {
                result.Append(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            }
            result.Append(sms.SenderName + " (" + sms.SenderTelephone + ") -> " + sms.ReceiverName + " (" + sms.ReceiverTelephone + ")" + Environment.NewLine);
            result.Append(sms.DateSent.Ddmmyyyyhhmmss(false) + Environment.NewLine);
            result.Append("\"" + sms.Message + "\"");
        }
        return result.ToString();
    }

}