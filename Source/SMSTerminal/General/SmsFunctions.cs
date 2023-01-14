using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using SMSTerminal.PDU;

namespace SMSTerminal.General;



public static class SmsFunctions
{
    public const string DateFormat = "dd.MM.yyyy";
    public const string DateTimeFormatConcat = "HH:mm:ss";
    public const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss";


    public static string MakeInternationalFormat(string tph, string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            throw new ArgumentException("Country code cannot be null or empty. (MakeInternationalFormat)");
        }

        if (!tph.IsValidTph())
        {
            return null;
        }

        if (tph.StartsWith("+"))
        {
            return tph;
        }

        return tph.Replace("-", "").Replace(" ", "").Substring(1).Insert(0, "+" + countryCode);
    }

    public static string DateTimeAddDays(int days)
    {
        return DateTime.Now.AddDays(days).ToString(DateFormat);
    }

    public static int GetMaxLength(int maxLengthInBytes, SMSEncoding smsEncoding)
    {
        var result = 0;
        switch (smsEncoding)
        {
            case SMSEncoding._7bit:
            {
                result = maxLengthInBytes;
            }
                break;
            case SMSEncoding._8bit:
            {
                result = maxLengthInBytes;
            }
                break;
            case SMSEncoding._UCS2:
            {
                result = maxLengthInBytes / 2;
            }
                break;
        }

        return result;
    }

    public static int CalculateLength(string message, SMSEncoding smsEncoding)
    {
        var result = 0;
        if (string.IsNullOrEmpty(message))
        {
            return result;
        }

        switch (smsEncoding)
        {
            case SMSEncoding._7bit:
            {
                result = message.Length;
            }
                break;
            case SMSEncoding._8bit:
            {
                result = message.Length;
            }
                break;
            case SMSEncoding._UCS2:
            {
                result = message.Length * 2;
            }
                break;
        }

        return result;
    }

    public static bool StatusReportCDSIsComplete(string data, out string cdsData)
    {
        cdsData = "";
        if (data == null || !data.Contains(ATMarkers.NewStatusReportArrived)) return false;

        //"\r\r+CDS: 24\r\r<pdu>\r\r\rAT+CMGF=0;+CMGS=39\r\r\r\r\r\r\r\r>"
        var regex = new Regex(@"^(\s*?)\+CDS: (\d{1,}\s{1,}[0-9 A-F]{10,}[\r]{1,})", RegexOptions.None);
        if (regex.IsMatch(data))
        {
            cdsData = regex.Matches(data)[0].Value;
        }
        return regex.IsMatch(data);
    }
}