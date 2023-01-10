namespace SMSTerminal.PDU;

public class PDUTimeStamp
{
    private readonly int _year;
    private readonly int _month;
    private readonly int _day;
    private readonly int _hour;
    private readonly int _minute;
    private readonly int _second;
    private readonly int _timezone;// in minutes

    public PDUTimeStamp(string timeStamp)
    {
        if (string.IsNullOrEmpty(timeStamp) || timeStamp.Length != 14)
        {
            throw new ArgumentException("Invalid PDU TimeStamp, must be length 14 to be processed.");
        }
        var swapped = PDUFunctions.SwapNibbles(timeStamp);
        _year = 2000 + int.Parse(swapped[..2]);
        swapped = swapped[2..];

        _month = int.Parse(swapped[..2]);
        swapped = swapped[2..];

        _day = int.Parse(swapped[..2]);
        swapped = swapped[2..];

        _hour = int.Parse(swapped[..2]);
        swapped = swapped[2..];

        _minute = int.Parse(swapped[..2]);
        swapped = swapped[2..];

        _second = int.Parse(swapped[..2]);
        swapped = swapped[2..];

        //GSM 24008.760 Ch. 10.5.3.9 The purpose of the timezone part of this information element is to encode the offset between universal time and local time in steps of 15 minutes.
        _timezone = 15 * int.Parse(swapped[..2]);//new TimeSpan(0, 15 * int.Parse(swapped.Substring(0, 2)), 0);
    }

    public DateTimeOffset GetDateTimeOffset()
    {
        return new DateTimeOffset(_year, _month, _day, _hour, _minute, _second,new TimeSpan(0,_timezone,0));
    }

    public override string ToString()
    {
        return string.Format(Environment.NewLine + 
                             "PDUTimeStamp: " + Environment.NewLine + 
                             "----------------" + Environment.NewLine + 
                             "Hour: {0}" + Environment.NewLine +
                             "Minute: {1}" + Environment.NewLine +
                             "Second: {2}" + Environment.NewLine + 
                             "Day: {3}" + Environment.NewLine + 
                             "Month: {4}" + Environment.NewLine +
                             "Year: {5}" + Environment.NewLine + 
                             "Timezone (in minutes): {6}" + Environment.NewLine +
                             "DateTimeOffset = " + GetDateTimeOffset()  + Environment.NewLine +
                             "----------------" + Environment.NewLine, _hour, _minute, _second, _day, _month, _year, _timezone);
    }
}