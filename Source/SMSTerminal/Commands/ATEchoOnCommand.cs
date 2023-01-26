using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// Sets echo on for the modem. SMSTerminal must have this on to work.
/// </summary>
internal class ATEchoOnCommand : ATCommand
{
    public ATEchoOnCommand(IModem modem)
    {
        Modem = modem;
        CommandType = "[AT Echo On Command]";
        /*
         * First command returns no echo, therefore the 2nd same command.
         */
        ATCommandsList.Add(new ATCommandLine(ATCommands.ATSetEchoOn, ATCommands.ATEndPart));
        ATCommandsList.Add(new ATCommandLine(ATCommands.ATSetEchoOn, ATCommands.ATEndPart));
    }

    public override async Task<CommandProgress> Process(ModemData modemData)
    {
        try
        {
            //Give modem some breathing space. SMS is slow communication.
            await Task.Delay(ModemTimings.MS100);

            switch (CommandIndex)
            {
                case 0:
                    {
                        /*
                         * Echo should be on now, check a 2nd time to make sure.
                         */
                        if (modemData.ModemResult == ModemResultEnum.Ok)
                        {
                            return CommandProgress.NextCommand;
                        }
                        return CommandProgress.Error;
                    }
            }

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