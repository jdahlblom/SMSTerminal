using System.IO.Ports;
using System.Text;
using NLog;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Modem;

/// <summary>
/// Handles reading from serial port.
/// </summary>
internal class SerialReceiver : ISerialReceiver
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IOutputParser _messageParser;

    public SerialReceiver(IOutputParser messageParser)
    {
        _messageParser = messageParser;
    }

    public IModem Modem { get; set; }
    public SerialPort SerialPort { get; set; }
    private readonly StringBuilder _incomingData = new();



    public async void ReceiveTextOverSerial(object sender, SerialDataReceivedEventArgs e)
    {
        while (Modem.Signals.IsActive(SignalType.ReadingModem, "ReceiveTextOverSerial"))
        {
            await Task.Delay(ModemTimings.MS200);
        }
        Modem.Signals.SetStarted(SignalType.ReadingModem);

        switch (e.EventType)
        {
            case SerialData.Chars:
            {
                try
                {
                    Logger.Debug($"SerialPort.Buffer = {SerialPort.BytesToRead}");
                    var byteArray = new byte[SerialPort.BytesToRead];
                    var cts = new CancellationTokenSource(ModemTimings.MS1000);
                    var bytesRead = await SerialPort.BaseStream.ReadAsync(byteArray, 0, byteArray.Length, cts.Token);

                    _incomingData.Append(Common.UsedEncoding.GetString(byteArray, 0, bytesRead));
                    var outputData = await _messageParser.ParseModemOutput(_incomingData.ToString());

                    if (outputData.Length != _incomingData.Length)
                    {
                        /*
                         * Output parser has removed and processed a complete message from _incomingData.
                         * The remaining part we set as our buffer as it may be a incomplete message.
                         */
                        _incomingData.Clear();
                        _incomingData.Append(outputData);
                    }
                }
                catch (TimeoutException t)
                {
                    if (!string.IsNullOrEmpty(_incomingData.ToString().RemoveAtLineEndings()))
                    {
                        var message =
                            $"{Modem.ModemId} Timeout when reading from SerialPort. Message = {t.DecodeException()} \n\n->{_incomingData}<-";
                        Logger.Error(message);
                        ModemEventManager.ModemEvent(this, Modem.ModemId, message, ModemEventType.ReceiveData,
                            Modem.ModemId, ModemResultEnum.TimeOutError);
                    }
                }
                catch (IOException t)
                {
                    var message =
                        $"{Modem.ModemId} IOException when reading from SerialPort. Message = {t.Message} \n\n->{_incomingData}<-";
                    Logger.Error(message);
                    ModemEventManager.ModemEvent(this, Modem.ModemId, message, ModemEventType.ReceiveData, Modem.ModemId,
                        ModemResultEnum.IOError);
                }
                catch (Exception t)
                {
                    var message =
                        $"{Modem.ModemId} Exception when reading from SerialPort. Message = {t.Message} \n\n->{_incomingData}<-";
                    Logger.Error(message);
                    ModemEventManager.ModemEvent(this, Modem.ModemId, message, ModemEventType.ReceiveData, Modem.ModemId,
                        ModemResultEnum.Error);
                }

                break;
            }
            case SerialData.Eof:
            {
                /*
                 * This triggers always when sending SMS because end of command is terminated with CTRL-Z and
                 * this is echoed back here.
                 */
                Logger.Debug("{0} \n****** EOF ******\n->{1}<-", Modem.ModemId, _incomingData);
                break;
            }
            default:
            {
                var message = "Socket switch statement defaulted.";
                Logger.Error(message);
                ModemEventManager.ModemEvent(this, Modem.ModemId, message, ModemEventType.ReceiveData, Modem.ModemId,
                    ModemResultEnum.Error);
                break;
            }
        }

        Modem.Signals.SetEnded(SignalType.ReadingModem);
    }
}