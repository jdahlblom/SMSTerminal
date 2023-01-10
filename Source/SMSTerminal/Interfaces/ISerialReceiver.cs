using System.IO.Ports;

namespace SMSTerminal.Interfaces;

/// <summary>
/// Receives the modem output from the serial port.
/// </summary>
internal interface ISerialReceiver
{
    IModem Modem { get; set; }
    SerialPort SerialPort { get; set; }
    void ReceiveTextOverSerial(object sender, SerialDataReceivedEventArgs e);
}