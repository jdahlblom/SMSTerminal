using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands;

/// <summary>
/// Simple AT "ping" command to see that the
/// modem is up and running.
/// </summary>
internal class ATQueryCommand : ATCommandBase
{
    public ATQueryCommand(IModem modem)
    {
        Modem = modem;
        CommandType = "[AT Test Command]";
        var command = new ATCommand(ATCommands.ATQueryCommand, ATCommands.ATEndPart);
        ATCommandsList.Add(command);
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