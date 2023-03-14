using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// This speeds up SMS execution keeping relay link open between sending.
/// </summary>
internal class ATKeepSMSRelayLinkOpen : ATCommand
{
    public ATKeepSMSRelayLinkOpen(IModem modem)
    {
        Modem = modem;
        CommandType = "[AT SMS Relay Link Command]";
        ATCommandsList.Add(new ATCommandLine(ATCommands.ATKeepSMSRelayLinkOpen, ATCommands.ATEndPart));
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