﻿namespace SMSTerminal.Modem;

/// <summary>
/// Delays and timeouts used throughout the application.
/// Tweak them according to modem performance.
/// </summary>
public static class ModemTimings
{
    public const int MS100 = 100;
    public const int MS200 = 200;
    public const int MS300 = 300;
    public const int MS400 = 400;
    public const int MS500 = 500;
    public const int MS600 = 600;
    public const int MS700 = 700;
    public const int MS800 = 800;
    public const int MS900 = 900;
    public const int MS1000 = 1000;


    /// <summary>
    /// Delay after restart command before applying settings again.
    /// </summary>
    public static int ModemRestartWait { get; set; } = 15000;

    /// <summary>
    /// After writing PDU the modem can be slow to respond.
    /// </summary>
    public static int ModemWriteTimeout { get; set; } = 5000;

    /// <summary>
    /// Modem can take quite a bit of time (!) before being up again after this command
    /// </summary>
    public static int WaitAfterSettingPIN { get; set; } = 2000;

    /// <summary>
    /// General modem reply wait time. Older modems can be very slow
    /// when sending SMS to complete.
    /// </summary>
    public static int ModemReplyWait { get; set; } = 15000;

    /// <summary>
    /// The wait time for a ModemDataMessage from the Channel.
    /// Since the response from  the modem can be slow this
    /// must be high enough.
    /// </summary>
    public static int ChannelReadWait { get; set; } = 400;

    /// <summary>
    /// CSMS, orphaned concatenated SMS that are fragmented will
    /// be deleted after this period.
    /// </summary>
    public static long CSMSMaxAgeMilliSecs { get; set; } = 120000;

    /// <summary>
    /// Days an SMS is valid.
    /// </summary>
    public static int SMSDaysValid { get; set; } = 4;

    /// <summary>
    /// Wait time after opening the serial port. Some modems turns
    /// on only after getting signal via RS232 and can take quite
    /// some time to be ready for AT commands.
    /// </summary>
    public static int WaitAfterSerialPortOpen { get; set; } = 5000;
}