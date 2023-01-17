using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// Command used to deliberately cause error on the modem side.
/// Used to see application behaviour.
/// </summary>
internal class ATErrorCommand : ATCommand
{
    public ATErrorCommand(IModem modem)
    {
        Modem = modem;
        CommandType = "[AT Error Command]";
        ATCommandsList.Add(new ATCommandLine(ATCommands.ATForceError, ATCommands.ATEndPart));
    }

    public override async Task<CommandProgress> Process(ModemData modemData)
    {
        try
        {
            //Give modem some breathing space. SMS is slow communication.
            await Task.Delay(ModemTimings.MS100);

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