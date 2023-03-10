using System.IO.Ports;
using SMSTerminal.General;

namespace SMSTerminal.Modem;

/// <summary>
/// Holds basic serial port and modem configurations.
/// </summary>
public class GsmModemConfig
{
    public string ComPort { get; set; }
    /// <summary>
    /// Set this and incoming SMS will show receiver (modem) telephone number
    /// </summary>
    public string ModemTelephoneNumber { get; set; } = "000000000";
    public BaudRate BaudRate { get; set; }
    public int DataBits { get; set; }
    public StopBits Stopbits { get; set; } = StopBits.None;
    public Parity Parity { get; set; }
    public int WriteTimeout { get; set; } = 30000;
    public int ReadTimeout { get; set; } = 30000;
    public bool LineSignalDtr { get; set; }
    public bool LineSignalRts { get; set; }
    public Handshake Handshake { get; set; }
    public string ModemManufacturer { get; set; }
    public string ModemModel { get; set; }
    public string IMSI { get; set; }
    public string ICCID { get; set; }
    public string ModemId => $"{ModemManufacturer}.{ModemModel}@{ComPort}";
    //public string ModemId => $"{IMSI}@{ComPort}";
    public bool Enabled { get; set; }
    public string PIN1 { get; set; }
    public bool DeleteSMSFromModemWhenRead { get; set; }
    public string CallForwardingTelephone { get; set; }
    public bool UseCallForwarding { get; set; }
    public bool AutoDisconnectIncomingCall { get; set; }


    public static GsmModemConfig Consume(GsmModemConfig gsmModemConfig)
    {
        var result = new GsmModemConfig
        {
            ComPort = gsmModemConfig.ComPort,
            BaudRate = gsmModemConfig.BaudRate,
            DataBits = gsmModemConfig.DataBits,
            Stopbits = gsmModemConfig.Stopbits,
            Parity = gsmModemConfig.Parity,
            LineSignalDtr = gsmModemConfig.LineSignalDtr,
            LineSignalRts = gsmModemConfig.LineSignalRts,
            Handshake = gsmModemConfig.Handshake,
            PIN1 = gsmModemConfig.PIN1,
            DeleteSMSFromModemWhenRead = gsmModemConfig.DeleteSMSFromModemWhenRead,
            AutoDisconnectIncomingCall = gsmModemConfig.AutoDisconnectIncomingCall,
            UseCallForwarding = gsmModemConfig.UseCallForwarding,
            CallForwardingTelephone = gsmModemConfig.CallForwardingTelephone
        };
        return result;
    }

    public void ClearModemModemInformation()
    {
        ModemManufacturer = "";
        ModemModel = "";
        IMSI = "";
        ICCID = "";
    }
}