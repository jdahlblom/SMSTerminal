namespace SMSTerminal.PDU
{
    public enum TypeOfNumber : byte
    {
        UNKNOWN = 0xC0,              // < C0 Bit 7 always 1, bit 6 = 0 -> UNKNOWN
        INTERNATIONAL = 0x10,
        NATIONAL = 0x20,
        NETWORK_SPECIFIC = 0x30,
        SUBSCRIBER = 0x40,
        ALPHANUMERIC = 0x50,
        ABBREVIATED = 0x60,
        RESERVED_FOR_EXTENSION = 0x70
    }

    public enum NumberingPlanIdentification : byte
    {
        UNKNOWN = 0x0,
        ISDN_OR_TELEPHONE = 0x1,
        X_121_DATA = 0x3,
        TELEX = 0x4,
        NATIONAL_NUMBERING = 0x8,
        PRIVATE_NUMBERING = 0x9,
        ERMES = 0xA,
        RESERVED_FOR_EXTENSION = 0xF
    }

    public class PDUTypeOfAddress
    {

        private readonly string _octets;
        private TypeOfNumber _typeOfNumber;
        private NumberingPlanIdentification _numberingPlanIdentification;
        private string _number;

        public PDUTypeOfAddress()
        {
        }

        public PDUTypeOfAddress(string octets)
        {
            _octets = octets;
        }

        public string Number
        {
            get => _number;
            set
            {
                _number = value;
                //Remove trailing F's
                if(_number != null && _number.Contains("F"))
                {
                    _number = _number.Replace("F","");
                }
            }
        }

        public void ParseOctets()
        {
            try
            {

                if (string.IsNullOrEmpty(_octets))
                {
                    throw new ArgumentException("Failed to parse octets. Octets null or empty.");
                }
                var octet = Convert.ToByte(_octets[..2], 16);
                Number = PDUFunctions.SwapNibbles(_octets[2..]);

                if (octet < 0x90)
                {
                    _typeOfNumber = TypeOfNumber.UNKNOWN;
                }
                else
                {
                    _typeOfNumber = (TypeOfNumber) (byte) (octet & 0x70);
                }
                if (_typeOfNumber == TypeOfNumber.ALPHANUMERIC)
                {
                    _numberingPlanIdentification = NumberingPlanIdentification.UNKNOWN;
                }
                else
                {
                    switch (_typeOfNumber)
                    {
                        case TypeOfNumber.UNKNOWN:
                        case TypeOfNumber.INTERNATIONAL:
                        case TypeOfNumber.NATIONAL:
                            {
                                _numberingPlanIdentification = (NumberingPlanIdentification) (byte) (octet & 0xF);
                            }
                            break;
                    }
                }
                if(_typeOfNumber == TypeOfNumber.INTERNATIONAL)
                {
                    _number = "+" + _number;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[PDUTypeOfAddress] {ex.Message}" );
            }
        }

        public byte GetOctet()
        {
            byte result = 0x80; //Leftmost bit always(!) set to 1
            if(_typeOfNumber != TypeOfNumber.UNKNOWN)
            {
                result = (byte) (result | (byte) _typeOfNumber);
            }
            result = (byte)(result | (byte)_numberingPlanIdentification);

            return result;
        }

        public override string ToString()
        {
            return string.Format("PDUTypeOfAddress:" + Environment.NewLine + 
                                 "--------------" + Environment.NewLine +
                                 "TypeOfNumber: {0}" + Environment.NewLine +
                                 "NumberingPlanIdentification: {1}" + Environment.NewLine +
                                 "Number: {2}" + Environment.NewLine +
                                 "--------------", _typeOfNumber, _numberingPlanIdentification, _number);
        }
    }
}
