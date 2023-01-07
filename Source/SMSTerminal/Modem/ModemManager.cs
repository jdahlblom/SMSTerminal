using NLog;
using SMSTerminal.Commands;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.Modem
{
    /// <summary>
    /// Handles modems, creates, adds, removes them.
    /// </summary>
    public class ModemManager : IDisposable, IModemMessageListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<IModem> _modems = new();
        public bool HasModems => _modems.Count > 0;


        public ModemManager()
        {
            ModemEventManager.AttachModemMessageListener(this);
        }

        public void Dispose()
        {
            ModemEventManager.DetachModemMessageListener(this);
            CloseTerminals();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// If configureAgain is false the modem will be removed, otherwise
        /// it will be reconfigured with the same parameters as it already has.
        /// </summary>
        /// <param name="modemId"></param>
        public async Task<bool> RestartModem(string modemId)
        {
            var found = false;
            var commandOK = false;
            GsmModemConfig gsmModemConfig = null;

            foreach (var modem in _modems.Where(modem => modem.ModemId == modemId))
            {
                found = true;
                gsmModemConfig = modem.GsmModemConfig;
                if (await modem.ExecuteCommand(new ATRestartModemCommand(modem)))
                {
                    commandOK = true;
                    _modems.Remove(modem);
                    modem.Dispose();
                }
                break;
            }

            if (!found)
            {
                return false;
            }

            return commandOK && await AddTerminal(gsmModemConfig, ModemTimings.ModemRestartWait);
        }

        /// <summary>
        /// For forcing error to see how the application behaves.
        /// </summary>
        public async void DoError(string modemId)
        {
            foreach (var modem in _modems.Where(modem => modem.ModemId == modemId))
            {
                await modem.ExecuteCommand(new ATErrorCommand(modem));
            }
        }

        public async void ModemMessageEvent(object sender, ModemMessageEventArgs e)
        {
            try
            {
                if (e.ModemMessageClass != ModemDataClassEnum.NewSMSWaiting) return;

                foreach (var modem in _modems.Where(modem => modem.ModemId == e.ModemId))
                {
                    var result = await modem.ExecuteCommand(new ATReadSMSCommand(modem, SMSReadStatus.Unread));
                    if (!result)
                    {
                        ModemEventManager.ModemEvent(this, modem.ModemId, "Failed to read new SMS.", ModemEventType.ReceiveData, "0", ModemResultEnum.Error);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        /// <summary>
        /// The ModemId (COM port) must be unique.
        /// </summary>
        /// <param name="gsmModemConfig"></param>
        /// <returns></returns>
        public async Task<bool> AddTerminal(GsmModemConfig gsmModemConfig)
        {
            return await AddTerminal(gsmModemConfig, ModemTimings.WaitAfterSerialPortOpen);
        }

        /// <summary>
        /// After restarting modem it seems to take longer for serial comms to work. Therefore the use of waitTimeAfterOpen.
        /// And much longer wait after restart.
        /// </summary>
        /// <param name="gsmModemConfig"></param>
        /// <param name="waitTimeAfterOpen"></param>
        /// <returns></returns>
        private async Task<bool> AddTerminal(GsmModemConfig gsmModemConfig, int waitTimeAfterOpen)
        {
            Modem modem = null;
            try
            {
                if (_modems.Exists(o => o.ModemId == gsmModemConfig.ModemId))
                {
                    throw new Exception($"Modem with that ModemId already exists. {gsmModemConfig.ModemId}");
                }

                modem = new Modem(gsmModemConfig);
                
                var outputParser = new OutputParser(modem);
                var serialReceiver = new SerialReceiver(outputParser)
                {
                    SerialPort = modem.SerialPort,
                    Modem = modem
                };
                modem.SerialReceiver = serialReceiver;
                await modem.Open(waitTimeAfterOpen);
                if (!await modem.ExecuteCommand(new ATQueryCommand(modem)))
                {
                    Logger.Error($"Modem {modem.ModemId} does not answer AT command. Modem not added to pool.");
                    modem.Dispose();
                    return false;
                }

                var pinCommand = new ATSetPINCommand(modem, gsmModemConfig.PIN1);
                if (!await modem.ExecuteCommand(pinCommand))
                {
                    Logger.Error($"Modem {modem.ModemId} could not set PIN. Modem not added to pool.");
                    modem.Dispose();
                    return false;
                }

                if (pinCommand.PINCodeSet)
                {
                    await Task.Delay(ModemTimings.WaitAfterSettingPIN);
                }

                if (!await modem.ExecuteCommand(new ATSetGSMPhase2Command(modem)))
                {
                    Logger.Error($"Modem {modem.ModemId} could not set GSM Phase 2. Modem not added to pool.");
                    modem.Dispose();
                    return false;
                }

                if (!await modem.ExecuteCommand(new ATSetVerboseErrorsCommand(modem)))
                {
                    Logger.Error($"Modem {modem.ModemId} could not set Verbose Errors. Modem not added to pool.");
                    modem.Dispose();
                    return false;
                }

                _modems.Add(modem);

                if (!await modem.ExecuteCommand(new ATReadSMSCommand(modem, SMSReadStatus.Unread)))
                {
                    Logger.Error($"Modem {modem.ModemId} failed to read new SMS.");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                modem?.Dispose();
                return false;
            }

            return true;
        }

        public List<string> GetModemList()
        {
            return _modems.Select(o => o.ModemId).ToList();
        }

        public async Task<bool> SendSMS(string modemId, OutgoingSms outgoingSms)
        {
            foreach (var modem in _modems.Where(modem => modem.ModemId == modemId))
            {
                return await modem.SendSMS(outgoingSms);
            }
            return false;
        }

        public async Task<bool> ReadNewSMS(string modemId)
        {
            foreach (var modem in _modems.Where(modem => modem.ModemId == modemId))
            {
                return await modem.ReadSMS(SMSReadStatus.Unread);
            }
            return false;
        }

        public async Task<bool> ReadOldSMS(string modemId)
        {
            foreach (var modem in _modems.Where(modem => modem.ModemId == modemId))
            {
                return await modem.ReadSMS(SMSReadStatus.Read);
            }
            return false;
        }

        public async Task<bool> GetNetworkStatus(string modemId)
        {
            foreach (var modem in _modems.Where(modem => modem.ModemId == modemId))
            {
                return await _modems[0].ExecuteCommand(new ATGetNetworkStatusCommand(_modems[0]));
            }
            return false;
        }

        public void CloseTerminals()
        {
            _modems.ForEach(o => o.Dispose());
            _modems.Clear();
        }

        public void CloseTerminal(string modemId)
        {
            _modems.Find(o => o.ModemId == modemId).Dispose();
            _modems.RemoveAll(o => o.ModemId == modemId);
        }

    }
}
