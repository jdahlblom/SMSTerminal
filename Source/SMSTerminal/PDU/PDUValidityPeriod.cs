namespace SMSTerminal.PDU
{
    public class PDUValidityPeriod
    {

        private int _validityPeriod;
        private readonly PDUHeader _pduHeader;

        public PDUValidityPeriod(PDUHeader pduHeader)
        {
            _pduHeader = pduHeader;
        }

        public PDUValidityPeriod(PDUHeader pduHeader, TimeSpan validityPeriod)
        {
            _pduHeader = pduHeader;
            ValidityPeriod = validityPeriod;
        }
        
        public PDUValidityPeriod(PDUHeader pduHeader, int validityPeriod)
        {
            _pduHeader = pduHeader;
            _validityPeriod = validityPeriod;
        }

        public TimeSpan ValidityPeriod
        {
            /*
                VP Value 	Validity period value
                0-143		(VP + 1) x 5 minutes (i.e 5 minutes intervals up to 12 hours)
                144-167		12 hours + ((VP-143) x 30 minutes)
                168-196		(VP-166) x 1 day
                197-255		(VP - 192) x 1 week
            */
            get
            {
                if (ValidityPeriodInt > 196)
                {
                    return new TimeSpan((ValidityPeriodInt - 192) * 7, 0, 0, 0);//Weeks
                }

                if (ValidityPeriodInt > 167)
                {
                    return new TimeSpan((ValidityPeriodInt - 166), 0, 0, 0);  //Days
                }

                if (ValidityPeriodInt > 143)
                {
                    return new TimeSpan(12, (ValidityPeriodInt - 143) * 30, 0); //Hours
                }

                return new TimeSpan(0, (ValidityPeriodInt + 1) * 5, 0);       //Minutes
            }
            set
            {
                if (value.Days > 441)
                {
                    throw new ArgumentOutOfRangeException($"ValidityPeriod : TimeSpan.Days = {value.Days}, value must be not greater 441 days.");
                }

                if (value.Days > 30) //Up to 441 days
                    ValidityPeriodInt = (byte)(192 + (value.Days / 7));
                else if (value.Days > 1) //Up to 30 days
                    ValidityPeriodInt = (byte)(166 + value.Days);
                else if (value.Hours > 12) //Up to 24 hours
                    ValidityPeriodInt = (byte)(143 + (value.Hours - 12) * 2 + value.Minutes / 30);
                else if (value.Hours > 1 || value.Minutes > 1) //Up to 12 days
                    ValidityPeriodInt = (byte)(value.Hours * 12 + value.Minutes / 5 - 1);
                else
                {
                    _pduHeader.ValidityPeriodFormatUsed = ValidityPeriodFormat.FieldNotPresent;
                    return;
                }
                _pduHeader.ValidityPeriodFormatUsed = ValidityPeriodFormat.Relative;
            }
        }

        public int ValidityPeriodInt
        {
            get => _validityPeriod;
            private set => _validityPeriod = value;
        }

        public override string ToString()
        {
            return string.Format("ValidityPeriod: {0}  | " + ValidityPeriod  + " (dd/hh/mm/ss) " + Environment.NewLine + 
                "PDUHeader: {1}", _validityPeriod, _pduHeader);
        }
    }
}
