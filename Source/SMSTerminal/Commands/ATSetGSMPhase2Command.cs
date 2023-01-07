using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    /// <summary>
    /// Enables GSM Phase 2 and enables unsolicited New Message Indication.
    /// With this SMSTerminal is notified automatically when there are new
    /// SMS waiting the be read.
    /// </summary>
    internal class ATSetGSMPhase2Command : ATCommandBase
    {
        public ATSetGSMPhase2Command(IModem modem)
        {
            Modem = modem;
            CommandType = "[Set GSM Phase 2 Command]";
            var command = new ATCommand(General.ATCommands.ATGSMPhase2Command, General.ATCommands.ATEndPart);
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
