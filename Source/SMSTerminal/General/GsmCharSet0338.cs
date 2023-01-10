using System.Collections;
using System.Text;

namespace SMSTerminal.General;

public class GsmCharSet0338
{

    private readonly SortedList _baseList = new();
    private readonly SortedList _extendedList = new();
    private bool _initialized;
    public static readonly char EscapeChar = (char)27;
    public static readonly byte EscapeByte = 27;

    public GsmCharSet0338()
    {
        InitLists();
    }

    public string GetString(byte[] byteArray)
    {
        InitLists();
        var result = new StringBuilder();
        var pos = 0;
        while (pos < byteArray.Length)
        {
            var b = byteArray[pos];
            if (b == EscapeByte && pos + 1 < byteArray.Length)//Safeguard when for some reason the last character in the byte array is escape char
            {
                result.Append((string)_extendedList[byteArray[pos + 1]]);
                pos++;
            }
            else
            {
                result.Append((string)_baseList[b]);
            }
            pos++;
        }
        return result.ToString();
    }

    public string FromGsm0338(byte[] byteArray)
    {
        InitLists();
        var result = new StringBuilder();
        foreach (var b in byteArray)
        {
            if (_baseList.ContainsKey(b))
            {
                result.Append((string)_baseList[b]);
            }
            else
            {
                result.Append('?');
            }
            /*else if (_extendedList.ContainsKey(b))
            {
                result.Append((string)_extendedList[b]);
            }*/
        }
        return result.ToString();
    }

    public bool IsBase(char s)
    {
        return _baseList.ContainsValue(s.ToString());
    }

    public bool IsExtended(char s)
    {
        return _extendedList.ContainsValue(s.ToString());
    }

    public bool IsBase(string s)
    {
        return _baseList.ContainsValue(s);
    }

    public bool IsExtended(string s)
    {
        return _extendedList.ContainsValue(s);
    }

    public static string ToGsm0338String(string textToConvert)
    {
        var array = ToGsm0338Bytes(textToConvert);
        return Encoding.GetEncoding(65001).GetString(array);
    }

    private static byte[] ToGsm0338Bytes(string textToConvert)
    {
        if (string.IsNullOrEmpty(textToConvert))
        {
            return null;
        }
        var gsmCharSet0338 = new GsmCharSet0338();
        var baseList = gsmCharSet0338._baseList;
        //var extendedList = gsmCharSet0338._extendedList;
        var result = new ArrayList();
        for (var i = 0; i < textToConvert.Length; i++)
        {
            var str = textToConvert.Substring(i, 1);
            if (baseList.ContainsValue(str))
            {
                result.Add(baseList.GetKey(baseList.IndexOfValue(str)));
            }
            /*
             * this cannot be used until comm mode is PDU. Escape char terminates the comm sequence 
            else if (extendedList.ContainsValue(str))
            {
                result.Add(escape char);
                result.Add(extendedList.GetKey(extendedList.IndexOfValue(str)));
            }*/
            else
            {
                byte.Parse("63", System.Globalization.NumberStyles.Integer);
            }
        }
        return result.ToArray(typeof(byte)) as byte[];
    }

    public static bool IsGsm0338(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return true;
        }
        var gsmCharSet0338 = new GsmCharSet0338();
        gsmCharSet0338.InitLists();
        foreach (var c in str)
        {
            if (!gsmCharSet0338.Contains(c.ToString()))
            {
                return false;
            }
        }
        return true;
    }

    private string PrintGsm0338CharSet(bool verbose)
    {
        var result = new StringBuilder();
        //BASE CHARACTERS
        if (verbose)
        {
            result.Append("Base characters:" + Environment.NewLine);
            result.Append("dec\tGSM character" + Environment.NewLine);
            foreach (byte b in _baseList.GetKeyList())
            {
                result.Append(b + "\t" + _baseList[b] + Environment.NewLine);
            }
            result.Append(Environment.NewLine);
            result.Append("Base characters:" + Environment.NewLine);
        }
        foreach (byte b in _baseList.GetKeyList())
        {
            result.Append(_baseList[b]);
        }
        result.Append(Environment.NewLine);
        //EXTENDED CHARACTERS
        if (verbose)
        {
            result.Append(Environment.NewLine);
            result.Append("Extended characters:" + Environment.NewLine);
            result.Append("dec\tGSM character" + Environment.NewLine);
            foreach (byte b in _extendedList.GetKeyList())
            {
                result.Append(b + "\t\\" + _extendedList[b] + Environment.NewLine);
            }
            result.Append(Environment.NewLine);
            result.Append("Extended characters:" + Environment.NewLine);
        }
        foreach (byte b in _extendedList.GetKeyList())
        {
            result.Append(_extendedList[b]);
        }
        return result.ToString();
    }

    public string GetDebugData(string textToConvert, bool onlyDiffs = false)
    {
        var encoding = Encoding.GetEncoding(28591);
        var result = new StringBuilder();
        var gsmCharSet0338 = new GsmCharSet0338();
        var baseList = gsmCharSet0338._baseList;
        for (var i = 0; i < textToConvert.Length; i++)
        {
            var str = textToConvert.Substring(i, 1);
            if (baseList.ContainsValue(str))
            {
                if ((byte)baseList.GetKey(baseList.IndexOfValue(str)) != encoding.GetBytes(str)[0])
                {
                    result.Append(@"(a) Converting character ->" + str + @"<- to ->" + baseList.GetKey(baseList.IndexOfValue(str)) + @"<-, default would be ->" + encoding.GetBytes(str)[0] + "<-  *********");
                    result.Append(Environment.NewLine);
                }
                else
                {
                    if (!onlyDiffs)
                    {
                        result.Append(@"(a) Converting character ->" + str + @"<- to ->" +
                                      baseList.GetKey(baseList.IndexOfValue(str)) + @"<-");
                        result.Append(Environment.NewLine);
                    }
                }
            }
            else
            {
                result.Append(@"(b) Converting character ->" + str + @"<- to ->" + byte.Parse("63", System.Globalization.NumberStyles.Integer) + @"<-, default would be ->" + encoding.GetBytes(str)[0] + "<-");
                result.Append(Environment.NewLine);
            }
        }
        return result.ToString();
    }

    public byte GetByte(char chr)
    {
        return GetByte(chr.ToString());
    }

    private bool Contains(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return true;
        }
        if (str.Length > 1)
        {
            throw new ArgumentException("Exception in GsmCharSet0338.Contains(), argument [string str] must be length 1.");
        }
        InitLists();
        if (str == "\t" || str == "`")
        {
            return true;
        }
        if (_baseList.ContainsValue(str))
        {
            return true;
        }
        if (_extendedList.ContainsValue(str))
        {
            return true;
        }
        return false;
    }

    private byte GetByte(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }

        if (str.Length > 1)
        {
            throw new ArgumentException("Exception in GsmCharSet0338.GetByte(), argument [string str] must be length 1.");
        }

        InitLists();

        str = str switch
        {
            "\t" => " ",
            "`" => "'",
            _ => str
        };

        if (_baseList.ContainsValue(str))
        {
            return (byte)_baseList.GetKey(_baseList.IndexOfValue(str));
        }

        if (_extendedList.ContainsValue(str))
        {
            return (byte)_extendedList.GetKey(_extendedList.IndexOfValue(str));
        }

        //All other unknown characters will be replaced with "."
        return (byte)_baseList.GetKey(_baseList.IndexOfValue("."));
    }

    public string GetBaseCharacter(char b)
    {
        return GetBaseCharacter((byte)b);
    }

    private string GetBaseCharacter(byte b)
    {
        InitLists();
        if (_baseList.ContainsKey(b))
        {
            return (string)_baseList[b];
        }
        return Functions.ByteArrayToString(new[] { b }, EncodingType.Utf8);
    }

    public string GetExtendedCharacter(char b)
    {
        return GetExtendedCharacter((byte)b);
    }

    private string GetExtendedCharacter(byte b)
    {
        InitLists();
        if (_extendedList.ContainsKey(b))
        {
            return (string)_extendedList[b];
        }
        return Functions.ByteArrayToString(new[] { b }, EncodingType.Utf8);
    }

    private void InitLists()
    {
        if (_initialized)
        {
            return;
        }
        _baseList.Add(byte.Parse("0", System.Globalization.NumberStyles.Integer), "@");
        _baseList.Add(byte.Parse("1", System.Globalization.NumberStyles.Integer), "£");
        _baseList.Add(byte.Parse("2", System.Globalization.NumberStyles.Integer), "$");
        _baseList.Add(byte.Parse("3", System.Globalization.NumberStyles.Integer), "¥");
        _baseList.Add(byte.Parse("4", System.Globalization.NumberStyles.Integer), "è");
        _baseList.Add(byte.Parse("5", System.Globalization.NumberStyles.Integer), "é");
        _baseList.Add(byte.Parse("6", System.Globalization.NumberStyles.Integer), "ù");
        _baseList.Add(byte.Parse("7", System.Globalization.NumberStyles.Integer), "ì");
        _baseList.Add(byte.Parse("8", System.Globalization.NumberStyles.Integer), "ò");
        _baseList.Add(byte.Parse("9", System.Globalization.NumberStyles.Integer), "Ç");
        _baseList.Add(byte.Parse("10", System.Globalization.NumberStyles.Integer), "\n");
        _baseList.Add(byte.Parse("11", System.Globalization.NumberStyles.Integer), "Ø");
        _baseList.Add(byte.Parse("12", System.Globalization.NumberStyles.Integer), "ø");
        _baseList.Add(byte.Parse("13", System.Globalization.NumberStyles.Integer), "\r");
        _baseList.Add(byte.Parse("14", System.Globalization.NumberStyles.Integer), "Å");
        _baseList.Add(byte.Parse("15", System.Globalization.NumberStyles.Integer), "å");
        _baseList.Add(byte.Parse("16", System.Globalization.NumberStyles.Integer), "\u0394");//"∆");
        _baseList.Add(byte.Parse("17", System.Globalization.NumberStyles.Integer), "_");
        _baseList.Add(byte.Parse("18", System.Globalization.NumberStyles.Integer), "\u03A6");//"Φ");
        _baseList.Add(byte.Parse("19", System.Globalization.NumberStyles.Integer), "\u0393");//"Γ");
        _baseList.Add(byte.Parse("20", System.Globalization.NumberStyles.Integer), "\u039B");//"Λ");
        _baseList.Add(byte.Parse("21", System.Globalization.NumberStyles.Integer), "\u03A9");//"Ω");
        _baseList.Add(byte.Parse("22", System.Globalization.NumberStyles.Integer), "\u03A0");//"Π");
        _baseList.Add(byte.Parse("23", System.Globalization.NumberStyles.Integer), "\u03A8");//"Ψ");
        _baseList.Add(byte.Parse("24", System.Globalization.NumberStyles.Integer), "\u03A3");//"Σ");
        _baseList.Add(byte.Parse("25", System.Globalization.NumberStyles.Integer), "\u0398");//"Θ");
        _baseList.Add(byte.Parse("26", System.Globalization.NumberStyles.Integer), "\u039E");//"Ξ");
        _baseList.Add(byte.Parse("27", System.Globalization.NumberStyles.Integer), ((char)27).ToString());//Escape Character
        _baseList.Add(byte.Parse("28", System.Globalization.NumberStyles.Integer), "Æ");
        _baseList.Add(byte.Parse("29", System.Globalization.NumberStyles.Integer), "æ");
        _baseList.Add(byte.Parse("30", System.Globalization.NumberStyles.Integer), "ß");
        _baseList.Add(byte.Parse("31", System.Globalization.NumberStyles.Integer), "É");
        _baseList.Add(byte.Parse("32", System.Globalization.NumberStyles.Integer), " ");
        _baseList.Add(byte.Parse("33", System.Globalization.NumberStyles.Integer), "!");
        _baseList.Add(byte.Parse("34", System.Globalization.NumberStyles.Integer), "\"");
        _baseList.Add(byte.Parse("35", System.Globalization.NumberStyles.Integer), "#");
        _baseList.Add(byte.Parse("36", System.Globalization.NumberStyles.Integer), "¤");
        _baseList.Add(byte.Parse("37", System.Globalization.NumberStyles.Integer), "%");
        _baseList.Add(byte.Parse("38", System.Globalization.NumberStyles.Integer), "&");
        _baseList.Add(byte.Parse("39", System.Globalization.NumberStyles.Integer), "'");
        _baseList.Add(byte.Parse("40", System.Globalization.NumberStyles.Integer), "(");
        _baseList.Add(byte.Parse("41", System.Globalization.NumberStyles.Integer), ")");
        _baseList.Add(byte.Parse("42", System.Globalization.NumberStyles.Integer), "*");
        _baseList.Add(byte.Parse("43", System.Globalization.NumberStyles.Integer), "+");
        _baseList.Add(byte.Parse("44", System.Globalization.NumberStyles.Integer), ",");
        _baseList.Add(byte.Parse("45", System.Globalization.NumberStyles.Integer), "-");
        _baseList.Add(byte.Parse("46", System.Globalization.NumberStyles.Integer), ".");
        _baseList.Add(byte.Parse("47", System.Globalization.NumberStyles.Integer), "/");
        _baseList.Add(byte.Parse("48", System.Globalization.NumberStyles.Integer), "0");
        _baseList.Add(byte.Parse("49", System.Globalization.NumberStyles.Integer), "1");
        _baseList.Add(byte.Parse("50", System.Globalization.NumberStyles.Integer), "2");
        _baseList.Add(byte.Parse("51", System.Globalization.NumberStyles.Integer), "3");
        _baseList.Add(byte.Parse("52", System.Globalization.NumberStyles.Integer), "4");
        _baseList.Add(byte.Parse("53", System.Globalization.NumberStyles.Integer), "5");
        _baseList.Add(byte.Parse("54", System.Globalization.NumberStyles.Integer), "6");
        _baseList.Add(byte.Parse("55", System.Globalization.NumberStyles.Integer), "7");
        _baseList.Add(byte.Parse("56", System.Globalization.NumberStyles.Integer), "8");
        _baseList.Add(byte.Parse("57", System.Globalization.NumberStyles.Integer), "9");
        _baseList.Add(byte.Parse("58", System.Globalization.NumberStyles.Integer), ":");
        _baseList.Add(byte.Parse("59", System.Globalization.NumberStyles.Integer), ";");
        _baseList.Add(byte.Parse("60", System.Globalization.NumberStyles.Integer), "<");
        _baseList.Add(byte.Parse("61", System.Globalization.NumberStyles.Integer), "=");

        _baseList.Add(byte.Parse("62", System.Globalization.NumberStyles.Integer), ">");
        _baseList.Add(byte.Parse("63", System.Globalization.NumberStyles.Integer), "?");
        _baseList.Add(byte.Parse("64", System.Globalization.NumberStyles.Integer), "¡");
        _baseList.Add(byte.Parse("65", System.Globalization.NumberStyles.Integer), "A");
        _baseList.Add(byte.Parse("66", System.Globalization.NumberStyles.Integer), "B");
        _baseList.Add(byte.Parse("67", System.Globalization.NumberStyles.Integer), "C");
        _baseList.Add(byte.Parse("68", System.Globalization.NumberStyles.Integer), "D");
        _baseList.Add(byte.Parse("69", System.Globalization.NumberStyles.Integer), "E");
        _baseList.Add(byte.Parse("70", System.Globalization.NumberStyles.Integer), "F");
        _baseList.Add(byte.Parse("71", System.Globalization.NumberStyles.Integer), "G");
        _baseList.Add(byte.Parse("72", System.Globalization.NumberStyles.Integer), "H");
        _baseList.Add(byte.Parse("73", System.Globalization.NumberStyles.Integer), "I");
        _baseList.Add(byte.Parse("74", System.Globalization.NumberStyles.Integer), "J");
        _baseList.Add(byte.Parse("75", System.Globalization.NumberStyles.Integer), "K");
        _baseList.Add(byte.Parse("76", System.Globalization.NumberStyles.Integer), "L");
        _baseList.Add(byte.Parse("77", System.Globalization.NumberStyles.Integer), "M");
        _baseList.Add(byte.Parse("78", System.Globalization.NumberStyles.Integer), "N");
        _baseList.Add(byte.Parse("79", System.Globalization.NumberStyles.Integer), "O");
        _baseList.Add(byte.Parse("80", System.Globalization.NumberStyles.Integer), "P");
        _baseList.Add(byte.Parse("81", System.Globalization.NumberStyles.Integer), "Q");
        _baseList.Add(byte.Parse("82", System.Globalization.NumberStyles.Integer), "R");
        _baseList.Add(byte.Parse("83", System.Globalization.NumberStyles.Integer), "S");
        _baseList.Add(byte.Parse("84", System.Globalization.NumberStyles.Integer), "T");
        _baseList.Add(byte.Parse("85", System.Globalization.NumberStyles.Integer), "U");
        _baseList.Add(byte.Parse("86", System.Globalization.NumberStyles.Integer), "V");
        _baseList.Add(byte.Parse("87", System.Globalization.NumberStyles.Integer), "W");
        _baseList.Add(byte.Parse("88", System.Globalization.NumberStyles.Integer), "X");
        _baseList.Add(byte.Parse("89", System.Globalization.NumberStyles.Integer), "Y");
        _baseList.Add(byte.Parse("90", System.Globalization.NumberStyles.Integer), "Z");
        _baseList.Add(byte.Parse("91", System.Globalization.NumberStyles.Integer), "Ä");
        _baseList.Add(byte.Parse("92", System.Globalization.NumberStyles.Integer), "Ö");
        _baseList.Add(byte.Parse("93", System.Globalization.NumberStyles.Integer), "Ñ");
        _baseList.Add(byte.Parse("94", System.Globalization.NumberStyles.Integer), "Ü");
        _baseList.Add(byte.Parse("95", System.Globalization.NumberStyles.Integer), "§");
        _baseList.Add(byte.Parse("96", System.Globalization.NumberStyles.Integer), "¿");
        _baseList.Add(byte.Parse("97", System.Globalization.NumberStyles.Integer), "a");
        _baseList.Add(byte.Parse("98", System.Globalization.NumberStyles.Integer), "b");
        _baseList.Add(byte.Parse("99", System.Globalization.NumberStyles.Integer), "c");
        _baseList.Add(byte.Parse("100", System.Globalization.NumberStyles.Integer), "d");
        _baseList.Add(byte.Parse("101", System.Globalization.NumberStyles.Integer), "e");
        _baseList.Add(byte.Parse("102", System.Globalization.NumberStyles.Integer), "f");
        _baseList.Add(byte.Parse("103", System.Globalization.NumberStyles.Integer), "g");
        _baseList.Add(byte.Parse("104", System.Globalization.NumberStyles.Integer), "h");
        _baseList.Add(byte.Parse("105", System.Globalization.NumberStyles.Integer), "i");
        _baseList.Add(byte.Parse("106", System.Globalization.NumberStyles.Integer), "j");
        _baseList.Add(byte.Parse("107", System.Globalization.NumberStyles.Integer), "k");
        _baseList.Add(byte.Parse("108", System.Globalization.NumberStyles.Integer), "l");
        _baseList.Add(byte.Parse("109", System.Globalization.NumberStyles.Integer), "m");
        _baseList.Add(byte.Parse("110", System.Globalization.NumberStyles.Integer), "n");
        _baseList.Add(byte.Parse("111", System.Globalization.NumberStyles.Integer), "o");
        _baseList.Add(byte.Parse("112", System.Globalization.NumberStyles.Integer), "p");
        _baseList.Add(byte.Parse("113", System.Globalization.NumberStyles.Integer), "q");
        _baseList.Add(byte.Parse("114", System.Globalization.NumberStyles.Integer), "r");
        _baseList.Add(byte.Parse("115", System.Globalization.NumberStyles.Integer), "s");
        _baseList.Add(byte.Parse("116", System.Globalization.NumberStyles.Integer), "t");
        _baseList.Add(byte.Parse("117", System.Globalization.NumberStyles.Integer), "u");
        _baseList.Add(byte.Parse("118", System.Globalization.NumberStyles.Integer), "v");
        _baseList.Add(byte.Parse("119", System.Globalization.NumberStyles.Integer), "w");
        _baseList.Add(byte.Parse("120", System.Globalization.NumberStyles.Integer), "x");
        _baseList.Add(byte.Parse("121", System.Globalization.NumberStyles.Integer), "y");
        _baseList.Add(byte.Parse("122", System.Globalization.NumberStyles.Integer), "z");
        _baseList.Add(byte.Parse("123", System.Globalization.NumberStyles.Integer), "ä");
        _baseList.Add(byte.Parse("124", System.Globalization.NumberStyles.Integer), "ö");
        _baseList.Add(byte.Parse("125", System.Globalization.NumberStyles.Integer), "ñ");
        _baseList.Add(byte.Parse("126", System.Globalization.NumberStyles.Integer), "ü");
        _baseList.Add(byte.Parse("127", System.Globalization.NumberStyles.Integer), "à");

        //Extended:
        _extendedList.Add(byte.Parse("10", System.Globalization.NumberStyles.Integer), "\f");
        _extendedList.Add(byte.Parse("20", System.Globalization.NumberStyles.Integer), "^");
        _extendedList.Add(byte.Parse("40", System.Globalization.NumberStyles.Integer), "{");
        _extendedList.Add(byte.Parse("41", System.Globalization.NumberStyles.Integer), "}");
        _extendedList.Add(byte.Parse("47", System.Globalization.NumberStyles.Integer), "\\");
        _extendedList.Add(byte.Parse("60", System.Globalization.NumberStyles.Integer), "[");
        _extendedList.Add(byte.Parse("61", System.Globalization.NumberStyles.Integer), "~");
        _extendedList.Add(byte.Parse("62", System.Globalization.NumberStyles.Integer), "]");
        _extendedList.Add(byte.Parse("64", System.Globalization.NumberStyles.Integer), "|");
        _extendedList.Add(byte.Parse("101", System.Globalization.NumberStyles.Integer), "€");

        _initialized = true;
    }
}