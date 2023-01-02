using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    internal class SetVerboseErrorsCommand : CommandBase
    {
        public SetVerboseErrorsCommand(IModem modem)
        {
            Modem = modem;
            CommandType = "[Set Verbose Errors Command]";
            var command = new Command(ATCommands.UseVerboseErrorsCommand, ATCommands.ATEndPart);
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
