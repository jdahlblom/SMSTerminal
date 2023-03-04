using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// ATCommand to be used when incoming call is detected.
///
/// Not all modem supports call forwarding. Disconnecting incoming call is therefore only supported.
/// </summary>
internal class ATDisconnectCallCommand : ATCommand
{
    public ATDisconnectCallCommand(IModem modem, bool disconnectIncomingCall, bool redirectIncomingCall, string redirectionNumber)
    {
        Modem = modem;
        CommandType = "[AT Disconnecting Incoming Call Command]";

        if (disconnectIncomingCall)
        {
            ATCommandsList.Add(new ATCommandLine(ATCommands.ATDisconnectIncomingCallCommand, ATCommands.ATEndPart));
        }

        /*if (redirectIncomingCall)
        {
            ATCommandsList.Add(new ATCommandLine(ATCommands.ATEnableCallForwardingCommandPart1 + redirectionNumber + ATCommands.ATEnableCallForwardingCommandPart2, ATCommands.ATEndPart));
        }*/
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

            if (HasNextATCommand) return CommandProgress.NextCommand;

        }
        catch (Exception e)
        {
            Logger.Error(e);
            return CommandProgress.Error;
        }

        return CommandProgress.Finished;
    }
}