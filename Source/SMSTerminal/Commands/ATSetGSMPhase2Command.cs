using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

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
        var command = new ATCommand(ATCommands.ATGSMPhase2Command, ATCommands.ATEndPart);
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