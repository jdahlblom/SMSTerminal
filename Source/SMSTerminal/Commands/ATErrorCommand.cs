using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    /// <summary>
    /// Command used to deliberately cause error on the modem side.
    /// Used to see application behaviour.
    /// </summary>
    internal class ATErrorCommand : ATCommandBase
    {
        public ATErrorCommand(IModem modem)
        {
            Modem = modem;
            CommandType = "[AT Error Command]";
            var command = new ATCommand(General.ATCommands.ATForceError, General.ATCommands.ATEndPart);
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
