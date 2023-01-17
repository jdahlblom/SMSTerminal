using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// Generic Command used to execute ad-hoc AT commands.
/// </summary>
internal class ATGenericCommand : ATCommand
{
    public ATGenericCommand(IModem modem, string atCommand, string terminationString)
    {
        Modem = modem;
        CommandType = "[AT Generic Command]";
        ATCommandsList.Add(new ATCommandLine(atCommand, terminationString));
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