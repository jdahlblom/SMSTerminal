using System.Collections;
using System.Text;
using SMSTerminal.General;

namespace SMSTerminal.PDU;

public enum CodingDirection : byte
{
    Encoding,
    Decoding
}

public enum MessageDirection : byte
{
    INCOMING,
    OUTGOING
}


/* TP-Status from 3GPP TS 23.040 section 9.2.3.15 */
public enum TpStatus
{
    /* SMS received successfully */
    TP_STATUS_RECEIVED_OK = 0x00,
    TP_STATUS_UNABLE_TO_CONFIRM_DELIVERY = 0x01,
    TP_STATUS_REPLACED = 0x02,
    /* Reserved: 0x03 - 0x0f */
    /* Values specific to each SC: 0x10 - 0x1f */
    /* Temporary error, SC still trying to transfer SM: */
    TP_STATUS_TRY_CONGESTION = 0x20,
    TP_STATUS_TRY_SME_BUSY = 0x21,
    TP_STATUS_TRY_NO_RESPONSE_FROM_SME = 0x22,
    TP_STATUS_TRY_SERVICE_REJECTED = 0x23,
    TP_STATUS_TRY_QOS_NOT_AVAILABLE = 0x24,
    TP_STATUS_TRY_SME_ERROR = 0x25,
    /* Reserved: 0x26 - 0x2f */
    /* Values specific to each SC: 0x30 - 0x3f */
    /* Permanent error, SC is not making any more transfer attempts:  */
    TP_STATUS_PERM_REMOTE_PROCEDURE_ERROR = 0x40,
    TP_STATUS_PERM_INCOMPATIBLE_DEST = 0x41,
    TP_STATUS_PERM_REJECTED_BY_SME = 0x42,
    TP_STATUS_PERM_NOT_OBTAINABLE = 0x43,
    TP_STATUS_PERM_QOS_NOT_AVAILABLE = 0x44,
    TP_STATUS_PERM_NO_INTERWORKING = 0x45,
    TP_STATUS_PERM_VALID_PER_EXPIRED = 0x46,
    TP_STATUS_PERM_DELETED_BY_ORIG_SME = 0x47,
    TP_STATUS_PERM_DELETED_BY_SC_ADMIN = 0x48,
    TP_STATUS_PERM_SM_NO_EXIST = 0x49,
    /* Reserved: 0x4a - 0x4f */
    /* Values specific to each SC: 0x50 - 0x5f */
    /* Temporary error, SC is not making any more transfer attempts: */
    TP_STATUS_TMP_CONGESTION = 0x60,
    TP_STATUS_TMP_SME_BUSY = 0x61,
    TP_STATUS_TMP_NO_RESPONSE_FROM_SME = 0x62,
    TP_STATUS_TMP_SERVICE_REJECTED = 0x63,
    TP_STATUS_TMP_QOS_NOT_AVAILABLE = 0x64,
    TP_STATUS_TMP_SME_ERROR = 0x65,
    /* Reserved: 0x66 - 0x6f */
    /* Values specific to each SC: 0x70 - 0x7f */
    /* Reserved: 0x80 - 0xff */
    TP_STATUS_NONE = 0xFF
}

public static class PDUFunctions
{
    /*
     * MAX MESSAGE LENGTH INFORMATION (characters)
     * 
     * SINGLE SMS:
     * 7-bit    8-bit   UTF-16
     *  160      140     70  
     * 
     * CONCATENATED SMS USING 8-bit message reference:
     * 7-bit    8-bit   UTF-16
     *  153      134     67
     */


    //These are only used for outgoing SMS, i.e. encoding SMS in SMSTerminal
    public const int Max7BitSingle = 160;
    public const int Max8BitSingle = 140;
    public const int MaxUtf16Single = 70;
    public const int Max7BitConcatenated = 152;         //CSMS Message Reference uses 2 bytes UDH = 7 bytes
    public const int Max8BitConcatenated16BitRef = 133; //CSMS Message Reference uses 2 bytes UDH = 7 bytes
    public const int MaxUtf16Concatenated16BitRef = 66; //CSMS Message Reference uses 2 bytes UDH = 7 bytes

    public static string ReverseString(string str)
    {
        var charArray = str.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static string SwapNibbles(string source)
    {
        return SwapNibbles(source, source.Length);
    }

    public static string SwapNibbles(string source, int length)
    {
        var result = string.Empty;
        for (var i = 0; i < length; i++)
        {
            result = result.Insert(i % 2 == 0 ? i : i - 1, source[i].ToString());
        }
        return result;
    }

    public static string ToSevenBits(byte b)
    {
        var bitsAsString = Convert.ToString(b, 2);
        return bitsAsString.PadLeft(7, '0');
    }

    public static string ToEightBits(byte b)
    {
        var bitsAsString = Convert.ToString(b, 2);
        return bitsAsString.PadLeft(8, '0');
    }

    public static byte FromSevenBits(string bits)
    {
        return Convert.ToByte(bits, 2);
    }

    public static string EncodeUCS2ToHex(string s)
    {
        var result = "";
        var bytes = Encoding.BigEndianUnicode.GetBytes(s);
        foreach (var b in bytes)
        {
            var str = Convert.ToString(b, 16).PadLeft(2, '0');
            result += str;
        } 
        return result.ToUpper();
    }

    public static byte[] EncodeUCS2(string s)
    {
        return Encoding.BigEndianUnicode.GetBytes(s);
    }

    public static string DecodeUCS2FromHex(string str)
    {
        var bytes = new ArrayList();
        var i = 0;
        //Console.WriteLine(str);
        while(i < str.Length)
        {
            //Console.WriteLine(str.Substring(i, 2));
            var b = Convert.ToByte(str.Substring(i,2), 16);
            bytes.Add(b);
            i += 2;
        }
        return Encoding.BigEndianUnicode.GetString(bytes.ToArray(typeof(byte)) as byte[]);
    }

    public static string Decode8BitHex(string message)
    {
        var bytes = Functions.HexStringToByteArray(message);
        if(bytes == null)
        {
            return "";
        }
        return Encoding.GetEncoding(28591).GetString(bytes);
    }

    public static string GetHexNibbleRepresentation(string hex)
    {
        return hex switch
        {
            "0" => "0000",
            "1" => "0001",
            "2" => "0010",
            "3" => "0011",
            "4" => "0100",
            "5" => "0101",
            "6" => "0110",
            "7" => "0111",
            "8" => "1000",
            "9" => "1001",
            "A" => "1010",
            "B" => "1011",
            "C" => "1100",
            "D" => "1101",
            "E" => "1110",
            "F" => "1111",
            _ => throw new ArgumentException($"Failed to get nibble for hex. {hex}")
        };
    }

    public static string GetNibbleHexRepresentation(string nibble)
    {
        return nibble switch
        {
            "0000" => "0",
            "0001" => "1",
            "0010" => "2",
            "0011" => "3",
            "0100" => "4",
            "0101" => "5",
            "0110" => "6",
            "0111" => "7",
            "1000" => "8",
            "1001" => "9",
            "1010" => "A",
            "1011" => "B",
            "1100" => "C",
            "1101" => "D",
            "1110" => "E",
            "1111" => "F",
            _ => throw new ArgumentException($"Failed to get hex for nibble. {nibble}")
        };
    }

    public static string GetFriendlyTpStatusMessage(TpStatus tpStatus)
    {
        switch (tpStatus)
        {
            /* SMS received successfully */
            case TpStatus.TP_STATUS_RECEIVED_OK:
            {
                return "Message received OK.";
            }
            case TpStatus.TP_STATUS_UNABLE_TO_CONFIRM_DELIVERY:
            {
                return "Unable to confirm delivery.";
            }
            case TpStatus.TP_STATUS_REPLACED:
            {
                return "Message replaced.";
            }
            /* Reserved: 0x03 - 0x0f */
            /* Values specific to each SC: 0x10 - 0x1f */
            /* Temporary error, SC still trying to transfer SM: */
            case TpStatus.TP_STATUS_TRY_CONGESTION:
            {
                return "Trying to send, congestion.";
            }
            case TpStatus.TP_STATUS_TRY_SME_BUSY:
            {
                return "Trying to send, recipient busy.";
            }
            case TpStatus.TP_STATUS_TRY_NO_RESPONSE_FROM_SME:
            {
                return "Trying to send, no response from recipient.";
            }
            case TpStatus.TP_STATUS_TRY_SERVICE_REJECTED:
            {
                return "Trying to send, service rejected.";
            }
            case TpStatus.TP_STATUS_TRY_QOS_NOT_AVAILABLE:
            {
                return "Trying to send, QOS not available.";
            }
            case TpStatus.TP_STATUS_TRY_SME_ERROR:
            {
                return "Trying to send, recipient error.";
            }
            /* Reserved: 0x26 - 0x2f */
            /* Values specific to each SC: 0x30 - 0x3f */
            /* Permanent error, SC is not making any more transfer attempts:  */
            case TpStatus.TP_STATUS_PERM_REMOTE_PROCEDURE_ERROR:
            {
                return "Permanent error, remote procedure error.";
            }
            case TpStatus.TP_STATUS_PERM_INCOMPATIBLE_DEST:
            {
                return "Permanent error, incompatible destination.";
            }
            case TpStatus.TP_STATUS_PERM_REJECTED_BY_SME:
            {
                return "Permanent error, rejected by recipient.";
            }
            case TpStatus.TP_STATUS_PERM_NOT_OBTAINABLE:
            {
                return "Permanent error, not obtainable.";
            }
            case TpStatus.TP_STATUS_PERM_QOS_NOT_AVAILABLE:
            {
                return "Permanent error, QOS not available.";
            }
            case TpStatus.TP_STATUS_PERM_NO_INTERWORKING:
            {
                return "Permanent error, no interworking.";
            }
            case TpStatus.TP_STATUS_PERM_VALID_PER_EXPIRED:
            {
                return "Permanent error, validity period expired.";
            }
            case TpStatus.TP_STATUS_PERM_DELETED_BY_ORIG_SME:
            {
                return "Permanent error, deleted by sender.";
            }
            case TpStatus.TP_STATUS_PERM_DELETED_BY_SC_ADMIN:
            {
                return "Permanent error, deleted by service center administrator.";
            }
            case TpStatus.TP_STATUS_PERM_SM_NO_EXIST:
            {
                return "Permanent error, short message (SM) does not exists.";
            }
            /* Reserved: 0x4a - 0x4f */
            /* Values specific to each SC: 0x50 - 0x5f */
            /* Temporary error, SC is not making any more transfer attempts: */
            case TpStatus.TP_STATUS_TMP_CONGESTION:
            {
                return "Temporary error, congestion.";
            }
            case TpStatus.TP_STATUS_TMP_SME_BUSY:
            {
                return "Temporary error, recipient busy.";
            }
            case TpStatus.TP_STATUS_TMP_NO_RESPONSE_FROM_SME:
            {
                return "Temporary error, no response from recipient.";
            }
            case TpStatus.TP_STATUS_TMP_SERVICE_REJECTED:
            {
                return "Temporary error, service rejected.";
            }
            case TpStatus.TP_STATUS_TMP_QOS_NOT_AVAILABLE:
            {
                return "Temporary error, QOS not available.";
            }
            case TpStatus.TP_STATUS_TMP_SME_ERROR:
            {
                return "Trying to send, recipient error.";
            }
            /* Reserved: 0x66 - 0x6f */
            /* Values specific to each SC: 0x70 - 0x7f */
            /* Reserved: 0x80 - 0xff */
            case TpStatus.TP_STATUS_NONE:
            {
                return "Status none or unknown (TP_STATUS_NONE)";
            }
            default :
            {
                return "Status none or unknown (TP_STATUS_NONE)";
            }
        }
    }
}