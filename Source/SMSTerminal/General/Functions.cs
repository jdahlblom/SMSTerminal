using System.Text;

namespace SMSTerminal.General
{
    public static class Functions
    {
        private static readonly object LockRandom = new();
        private static readonly Random SMSTerminalRandom = new();

        public static string ByteArrayToHexString(byte[] byteArray)
        {
            if (byteArray == null)
            {
                return null;
            }

            var hexStringBuilder = new StringBuilder(byteArray.Length * 2);
            foreach (var b in byteArray)
            {
                hexStringBuilder.Append($"{b:x2}");
            }

            return hexStringBuilder.ToString();
        }


        public static byte[] HexStringToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
            {
                throw new ArgumentException($"Not a valid hex string (octets) {hex}");
            }

            var byteArray = new byte[hex.Length / 2];
            for (var i = 0; i < hex.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return byteArray;
        }


        public static string ConvertToHex(string asciiString)
        {
            var hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += string.Format("{0:x2}", Convert.ToUInt32(tmp.ToString()));
            }

            return hex;
        }

        public static IEnumerable<string> StringSplit(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public static string MakeLinesMaxLength(string str, int maxLength)
        {
            var stringList = StringSplit(str, maxLength);
            var result = new StringBuilder();
            foreach (var stringPart in stringList)
            {
                result.Append(stringPart + Environment.NewLine);
            }

            return result.ToString();
        }

        public static long MilliSecsNow()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static long DTOMilliSecsNow()
        {
            return DateTimeOffset.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static string ByteArrayToString(byte[] bytes, EncodingType encodingType)
        {
            Encoding encoding = null;
            switch (encodingType)
            {
                case EncodingType.Ascii:
                    encoding = new ASCIIEncoding();
                    break;
                case EncodingType.Unicode:
                    encoding = new UnicodeEncoding();
                    break;
                /*case EncodingType.Utf7:
                    encoding = new UTF7Encoding();//not secure, not to be used
                    break;*/
                case EncodingType.Utf8:
                    encoding = new UTF8Encoding();
                    break;
            }

            return encoding?.GetString(bytes);
        }

        public static int GetRandom(int min, int max)
        {
            lock (LockRandom)
            {
                return SMSTerminalRandom.Next(min, max);
            }
        }

        public static List<int> GetRandomList(int size, int min, int max)
        {

            if (max - min + 1 < size || size == 0)
            {
                throw new InvalidOperationException($"GetRandomList : invalid parameters. Cannot generate list with min = {min}, max = {max}, size = {size} params.");
            }

            var result = new List<int>();
            while (result.Count < size)
            {
                result.Add(GetRandom(min, max));
            }

            return result;
        }

    }
}
