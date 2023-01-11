using System.IO.Ports;
using System.Threading.Channels;
using NLog;
using SMSTerminal.Commands;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;
using SMSTerminal.Properties;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.Modem;

/// <summary>
/// Communicates with modem over RS232.
/// Executes commands.
/// </summary>
internal class Modem : IDisposable, IModem
{
    public bool HaltForPINError { get; set; }

    private readonly GsmModemConfig _gsmModemConfig;
    public string ModemId => _gsmModemConfig.ModemId;
    private readonly SerialPort _serialPort;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    internal Channel<ModemData> ModemDataChannel { get; } = Channel.CreateUnbounded<ModemData>();
    private readonly Channel<ModemData> _unknownModemDataChannel = Channel.CreateUnbounded<ModemData>();
    private ISerialReceiver _serialReceiver;
    public Signals Signals { get; } = new();

    public Modem(GsmModemConfig gsmModemConfig)
    {
        _gsmModemConfig = gsmModemConfig;
        _serialPort = new SerialPort
        {
            //Set standard values to avoid hanging
            ReadTimeout = 40000,
            WriteTimeout = 40000
        };
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _serialPort.DataReceived -= _serialReceiver.ReceiveTextOverSerial;
            _serialPort?.Close();
            _serialPort?.Dispose();
            _serialReceiver = null;
        }
    }

    private static void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public ISerialReceiver SerialReceiver
    {
        set => _serialReceiver = value;
    }

    public SerialPort SerialPort => _serialPort;
    public GsmModemConfig GsmModemConfig => _gsmModemConfig;

    public async Task Open(int waitAfterOpen)
    {
        _serialPort.DataReceived += _serialReceiver.ReceiveTextOverSerial;
        ApplyCommSettings();
        _serialPort.Open();

        await Task.Delay(waitAfterOpen);
    }

    public bool IsOpen => _serialPort is { IsOpen: true };

    public void Close()
    {
        Dispose();
    }
    
    private void ApplyCommSettings()
    {
        PDUMessageParser.ModemTelephone = _gsmModemConfig.ModemTelephoneNumber;

        _serialPort.PortName = _gsmModemConfig.ComPort;
        _serialPort.BaudRate = (int)_gsmModemConfig.BaudRate;
        _serialPort.Parity = _gsmModemConfig.Parity;
        _serialPort.StopBits = _gsmModemConfig.Stopbits;
        _serialPort.DataBits = _gsmModemConfig.DataBits;
        _serialPort.Handshake = _gsmModemConfig.Handshake;
        if (!_gsmModemConfig.LineSignalDtr && !_gsmModemConfig.LineSignalRts)
        {
            _serialPort.Handshake = Handshake.XOnXOff;
        }
        _serialPort.DtrEnable = _gsmModemConfig.LineSignalDtr;
        _serialPort.RtsEnable = _gsmModemConfig.LineSignalRts;
        _serialPort.WriteTimeout = _gsmModemConfig.WriteTimeout;
        _serialPort.ReadTimeout = _gsmModemConfig.ReadTimeout;
    }

    private bool PreFlightCheck()
    {
        if (HaltForPINError)
        {
            ModemEventManager.ModemEvent(this, ModemId, Resources.HaltForPin, ModemEventType.PIN, ModemId, ModemResultEnum.Critical);
            return false;
        }
        if (!_serialPort.IsOpen)
        {
            ModemEventManager.ModemEvent(this, ModemId, "Serial port is not open. Writing to port is currently not possible.", ModemEventType.ModemConfigurationStatus, ModemId, ModemResultEnum.Critical);
            return false;
        }

        return true;
    }

    private void SendEvent(string message, ModemResultEnum modemResultEnum)
    {
        ModemEventManager.ModemEvent(this, ModemId, message, ModemEventType.ModemComms, ModemId, modemResultEnum);
    }

    private string _previousCommand = "";
    public async Task<bool> ExecuteCommand(ICommand command)
    {
        try
        {
            //Not to choke the modem
            await Task.Delay(ModemTimings.MS300);

            while (Signals.IsActive(SignalType.ExecutingCommand, $"{command.CommandType} ExecuteCommand Wait"))
            {
                await Task.Delay(ModemTimings.MS200);
                Logger.Debug($"Last command was {_previousCommand}");
            }

            _previousCommand = command.CommandType;
            Signals.SetStarted(SignalType.ExecutingCommand);
            await WriteTextData(command.CurrentATCommand.ATCommandString + command.CurrentATCommand.TerminationString);
            var done = false;

            while (!done)
            {
                var cts = new CancellationTokenSource(ModemTimings.ModemReplyWait);
                var modemData = await ModemDataChannel.Reader.ReadAsync(cts.Token);
                Logger.Debug($"Modem sees \n->{modemData.Data}<-");
                switch (await command.Process(modemData))
                {
                    case CommandProgress.Finished:
                        {
                            done = true;
                            break;
                        }
                    case CommandProgress.NextCommand:
                        {
                            await WriteTextData(command.NextATCommand().ATCommandString + command.CurrentATCommand.TerminationString);
                            break;
                        }
                    case CommandProgress.NotExpectedDataReply:
                        {
                            var cts2 = new CancellationTokenSource(ModemTimings.ChannelReadWait);
                            //Todo, how should this be processed?
                            await _unknownModemDataChannel.Writer.WriteAsync(modemData, cts2.Token);
                            ModemEventManager.ModemEvent(this, ModemId, modemData.Data, ModemEventType.ModemComms, ModemId, ModemResultEnum.UnknownModemData);
                            break;
                        }
                    case CommandProgress.Error:
                        {
                            SendEvent($"{ModemId} Failed to execute {command.CommandType}.", ModemResultEnum.Error);
                            return false;
                        }
                    default:
                        {
                            throw new NotImplementedException();
                        }
                }
            }
        }
        catch (Exception e)
        {
            ModemEventManager.ModemEvent(this, ModemId, $"Failed to execute {command.CommandType} : {e.Message}", ModemEventType.ModemComms, ModemId, ModemResultEnum.Error);
            Logger.Error(e);
            return false;
        }
        finally
        {
            Signals.SetEnded(SignalType.ExecutingCommand);
        }

        return true;
    }

    public async Task<bool> WriteTextData(string text)
    {
        if (!PreFlightCheck())
        {
            return false;
        }

        try
        {
            if (text.RemoveAtLineEndings().Trim().Length > 0)
            {
                ModemEventManager.ModemEvent(this, ModemId, text.RemoveAtLineEndings(), ModemEventType.WriteData, "0", ModemResultEnum.None);
            }

            Logger.Debug("{0} PDUModem about to write ->{1}<-", ModemId, text);
            var byteArray = Common.UsedEncoding.GetBytes(text);

            while (Signals.IsActive(SignalType.ReadingModem, "WriteTextData Wait") || Signals.IsActive(SignalType.ModemWriting, "WriteTextData Wait"))
            {
                await Task.Delay(ModemTimings.MS200);
            }
            Signals.SetStarted(SignalType.ModemWriting);
            try
            {
                var cts = new CancellationTokenSource(ModemTimings.ModemWriteTimeout);
                await _serialPort.BaseStream.WriteAsync(byteArray, 0, byteArray.Length, cts.Token);
            }
            finally
            {
                Signals.SetEnded(SignalType.ModemWriting);
            }

        }
        catch (Exception e)
        {
            Logger.Error(e);
            return false;
        }

        return true;
    }

    public async Task<bool> SendSMS(OutgoingSms outgoingSms)
    {
        try
        {
            while (Signals.IsActive(SignalType.SendingSMS, $"{ModemId}.SendSMS Start") ||
                   Signals.IsActive(SignalType.ReadingSMS, $"{ModemId}.SendSMS Start"))
            {
                await Task.Delay(ModemTimings.MS300);
            }
            
            if (outgoingSms.ByteLength() > 160 && !await ExecuteCommand(new ATKeepSMSRelayLinkOpen(this)))
            {
                Logger.Error("Failed to set Keep SMS Relay Channel Open before sending SMS.");
            }
            Signals.SetStarted(SignalType.SendingSMS);
            var command = new ATSendSMSCommand(this, outgoingSms);
            return await ExecuteCommand(command);
        }
        finally
        {
            Signals.SetEnded(SignalType.SendingSMS);
        }
    }

    public async Task<bool> ReadSMS(SMSReadStatus smsReadStatus)
    {
        try
        {
            while (Signals.IsActive(SignalType.SendingSMS, $"{ModemId}.SendSMS Start") ||
                   Signals.IsActive(SignalType.ReadingSMS, $"{ModemId}.SendSMS Start"))
            {
                await Task.Delay(ModemTimings.MS300);
            }
            Signals.SetStarted(SignalType.ReadingSMS);
            var command = new ATReadSMSCommand(this, smsReadStatus);
            return await ExecuteCommand(command);
        }
        finally
        {
            Signals.SetEnded(SignalType.ReadingSMS);
        }
    }
    
    /*
    public void ApplyCallForwardingSettings()
    {
        
    }

    private CallForwardingStatus CheckCallForwardingOnModem()
    {
        
    }


    private void DisconnectIncomingCall()
    {
        
    }
    
    private bool ContainsIncomingCallIndication(string text)
    {
        
    }
    */
}

