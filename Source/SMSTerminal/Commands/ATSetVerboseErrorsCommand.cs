using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// Enables verbose errors for CME, CMS errors instead
/// of just reporting the error number.
/// </summary>
internal class ATSetVerboseErrorsCommand : ATCommand
{
    public ATSetVerboseErrorsCommand(IModem modem)
    {
        Modem = modem;
        CommandType = "[Set Verbose Errors Command]";
        ATCommandsList.Add(new ATCommandLine(ATCommands.UseVerboseErrorsCommand, ATCommands.ATEndPart));
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
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return CommandProgress.Error;
        }

        return CommandProgress.Finished;
    }
}