using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    internal class ATQueryCommand : CommandBase
    {
        public ATQueryCommand(IModem modem)
        {
            Modem = modem;
            CommandType = "[AT Test Command]";
            var command = new Command(ATCommands.ATQueryCommand, ATCommands.ATEndPart);
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
