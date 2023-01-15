using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// Returns network status, whether connected to mobile network or not.
/// </summary>
internal class ATGetNetworkStatusCommand : ATCommand
{
    public ATGetNetworkStatusCommand(IModem modem)
    {
        Modem = modem;
        CommandType = "[Get Network Status Command]";
        var command = new ATCommandLine(ATCommands.ATNetworkStatusRequestCommand, ATCommands.ATEndPart);
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
                
            if (modemData.HasError)
            {
                return CommandProgress.Error;
            }

            var networkStatus = ParseNetworkStatus(modemData.Data);
            SendEvent(networkStatus.ToString(), ModemEventType.NetworkStatus);
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return CommandProgress.Error;
        }

        return CommandProgress.Finished;
    }

    private GsmNetworkRegistrationStatus ParseNetworkStatus(string modemReply)
    {
        var result = GsmNetworkRegistrationStatus.Unknown;
            
        try
        {
            var status = int.Parse(modemReply.Substring(modemReply.IndexOf(",", StringComparison.Ordinal) + 1, 1));
            result = (GsmNetworkRegistrationStatus)status;
        }
        catch (Exception e)
        {
            Logger.Error("ParseNetworkStatus : " + e.DecodeException());
        }

        return result;
    }
}