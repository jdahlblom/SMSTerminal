using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    /// <summary>
    /// Retrieves information about modem & SIM.
    /// </summary>
    internal class ATGetModemInformationCommand : ATCommandBase
    {
        public string Manufacturer { get; private set; }
        public string Model { get; private set; }
        public string IMSI { get; private set; }
        public string ICCID { get; private set; }

        public ATGetModemInformationCommand(IModem modem)
        {
            Modem = modem;
            CommandType = "[Get Modem Information Command]";
            ATCommandsList.Add(new ATCommand(General.ATCommands.ATGetModemManufacturerCommand,General.ATCommands.ATEndPart));
            ATCommandsList.Add(new ATCommand(General.ATCommands.ATGetModemModelCommand, General.ATCommands.ATEndPart));
            ATCommandsList.Add(new ATCommand(General.ATCommands.ATGetIMSICommand, General.ATCommands.ATEndPart));
            ATCommandsList.Add(new ATCommand(General.ATCommands.ATGetICCIDCommand, General.ATCommands.ATEndPart));
        }

        public override CommandProgress Process(ModemData modemData)
        {
            try
            {
                if (!modemData.Data.Contains(ATCommandsList[CommandIndex].ATCommandString))
                {
                    return CommandProgress.NotExpectedDataReply;
                }

                SetModemDataForCurrentCommand(modemData);

                if (modemData.HasError)
                {
                    return CommandProgress.Error;
                }

                //AT+CGMI\nTelit\r\n\r\nOK\r\n
                ParseData(modemData.Data);
                return HasNextATCommand ? CommandProgress.NextCommand : CommandProgress.Finished;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return CommandProgress.Error;
            }

            return CommandProgress.Finished;
        }

        private void ParseData(string data)
        {
            var array = data.Trim().Split("\r", StringSplitOptions.RemoveEmptyEntries);
            switch (CommandIndex)
            {
                case 0:
                {
                    Manufacturer = array[1].Trim().RemoveAtLineEndings();
                    break;
                }
                case 1:
                {
                    Model = array[1].Trim().RemoveAtLineEndings();
                    break;
                }
                case 2:
                {
                    IMSI = array[1].Trim().RemoveAtLineEndings();
                    break;
                }
                case 3:
                {
                    ICCID = array[1].Trim().RemoveAtLineEndings();
                    break;
                }
                default:
                {
                    throw new ArgumentException($"Failed to handle index {CommandIndex}.");
                    break;
                }
            }
        }
    }
}
