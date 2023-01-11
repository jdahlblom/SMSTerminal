﻿using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands;

/// <summary>
/// Command used to deliberately cause error on the modem side.
/// Used to see application behaviour.
/// </summary>
internal class ATGenericCommand : ATCommandBase
{
    public ATGenericCommand(IModem modem, string atCommand, string terminationString)
    {
        Modem = modem;
        CommandType = "[AT Generic Command]";
        var command = new ATCommand(atCommand, terminationString);
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