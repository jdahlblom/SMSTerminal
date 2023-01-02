using System.IO.Ports;

namespace SMSTerminal.Interfaces
{
    internal interface ISerialReceiver
    {
        IModem Modem { get; set; }
        SerialPort SerialPort { get; set; }
        void ReceiveTextOverSerial(object sender, SerialDataReceivedEventArgs e);
    }
}
