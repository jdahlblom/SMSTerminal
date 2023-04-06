namespace SMSTerminal.General
{
    internal static class BitOperations
    {
        public static string GetBitString(List<string> bitStrings, bool pad)
        {
            if (pad)
            {
                return bitStrings.Aggregate((current, s) => $"{current}{Environment.NewLine}{s.PadLeft(8, '0')}");
            }

            return bitStrings.Aggregate((current, s) => $"{current}{Environment.NewLine}{s}");
        }

        public static string GetBitString(byte[] bytes)
        {
            return bytes.Aggregate("", (current, b) => current + (Convert.ToString(b, 2) + Environment.NewLine));
        }

        public static string GetBitString(List<byte> bytes, bool pad, bool padSeven)
        {
            if (pad && !padSeven)
            {
                return bytes.Aggregate("", (current, b) => current + (Convert.ToString(b, 2).PadLeft(8, '0') + Environment.NewLine));
            }
            if (pad)
            {
                return bytes.Aggregate("", (current, b) => current + (Convert.ToString(b, 2).PadLeft(7, '0') + Environment.NewLine));
            }
            return bytes.Aggregate("", (current, b) => current + (Convert.ToString(b, 2) + Environment.NewLine));
        }
    }
}