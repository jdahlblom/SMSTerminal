using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    internal class ATRestartModemCommand : CommandBase
    {
        public ATRestartModemCommand(IModem modem)
        {
            Modem = modem;
            CommandType = "[AT Restart Modem Command]";
            var command = new Command(ATCommands.ATRestartModem, ATCommands.ATEndPart);
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
