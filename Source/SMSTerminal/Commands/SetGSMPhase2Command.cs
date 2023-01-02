using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    internal class SetGSMPhase2Command : CommandBase
    {
        public SetGSMPhase2Command(IModem modem)
        {
            Modem = modem;
            CommandType = "[Set GSM Phase 2 Command]";
            var command = new Command(ATCommands.ATGSMPhase2Command, ATCommands.ATEndPart);
            ModemCommandsList.Add(command);
        }

        public override CommandProgress Process(ModemData modemData)
        {
            try
            {
                if (!modemData.Data.Contains(ModemCommandsList[CommandIndex].CommandString))
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
