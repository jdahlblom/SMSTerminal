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

    public static int GetEncodingIndex(SMSEncoding smsEncoding)
    {
        switch (smsEncoding)
        {
            case SMSEncoding._7bit:
                {
                    return 0;
                }
            case SMSEncoding._8bit:
                {
                    return 1;
                }
            case SMSEncoding._UCS2:
                {
                    return 2;
                }
        }
        return 0;
    }

    // TODO
    /*
    public static SMSEncoding GetSelectedEncoding(ComboBox comboBox)
    {
        switch (comboBox.SelectedIndex)
        {
            case 0:
            {
                return SMSEncoding._7bit;
            }
            case 1:
            {
                return SMSEncoding._8bit;
            }
            case 2:
            {
                return SMSEncoding._UCS2;
            }
        }
        return SMSEncoding._7bit;
    }
    */
}


public static class FlagsHelper
{
    public static bool IsSet<T>(T flags, T flag) where T : struct
    {
        var flagsValue = (int)(object)flags;
        var flagValue = (int)(object)flag;

        return (flagsValue & flagValue) != 0;
    }

    public static void Set<T>(ref T flags, T flag) where T : struct
    {
        var flagsValue = (int)(object)flags;
        var flagValue = (int)(object)flag;

        flags = (T)(object)(flagsValue | flagValue);
    }

    public static void Unset<T>(ref T flags, T flag) where T : struct
    {
        var flagsValue = (int)(object)flags;
        var flagValue = (int)(object)flag;

        flags = (T)(object)(flagsValue & (~flagValue));
    }
}