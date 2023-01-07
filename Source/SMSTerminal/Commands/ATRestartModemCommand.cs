﻿using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    /// <summary>
    /// Restarts the modem.
    /// </summary>
    internal class ATRestartModemCommand : ATCommandBase
    {
        public ATRestartModemCommand(IModem modem)
        {
            Modem = modem;
            CommandType = "[AT Restart Modem Command]";
            var command = new ATCommand(General.ATCommands.ATRestartModem, General.ATCommands.ATEndPart);
            ATCommandsList.Add(command);
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
                SendResultEvent();
                if (modemData.HasError)
                {
                    return CommandProgress.Error;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return CommandProgress.Error;
            }

            return CommandProgress.Finished;
        }
    }
}
