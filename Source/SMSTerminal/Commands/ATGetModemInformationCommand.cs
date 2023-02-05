using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// Retrieves information about modem & SIM.
/// </summary>
internal class ATGetModemInformationCommand : ATCommand
{
    public string Manufacturer { get; private set; }
    public string Model { get; private set; }
    public string IMSI { get; private set; }
    public string ICCID { get; private set; }

    public ATGetModemInformationCommand(IModem modem)
    {
        Modem = modem;
        CommandType = "[Get Modem Information Command]";
        ATCommandsList.Add(new ATCommandLine("Get Modem Manufacturer Command", ATCommands.ATGetModemManufacturerCommand,ATCommands.ATEndPart));
        ATCommandsList.Add(new ATCommandLine("Get Modem Model Command", ATCommands.ATGetModemModelCommand, ATCommands.ATEndPart));
        ATCommandsList.Add(new ATCommandLine("Get Modem IMSI Command", ATCommands.ATGetIMSICommand, ATCommands.ATEndPart));
        ATCommandsList.Add(new ATCommandLine("Get Modem ICCID Command", ATCommands.ATGetICCIDCommand, ATCommands.ATEndPart));
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
                if (CommandIndex == 3)
                {
                    //Ignore, older modem(s) don't have this
                    ICCID = "Error";
                    return CommandProgress.Finished;
                }
                else
                {
                    return CommandProgress.Error;
                }
            }

            //AT+CGMI\nTelit\r\n\r\nOK\r\n
            ParseData(modemData.Data);
            return HasNextATCommand ? CommandProgress.NextCommand : CommandProgress.Finished;
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return CommandProgress.Error;
        }
    }

    private void ParseData(string data)
    {
        var array = data.Trim().Split("\r", StringSplitOptions.RemoveEmptyEntries);
        switch (CommandIndex)
        {
            case 0:
            {
                Manufacturer = array[1].Trim().RemoveAtLineEndings();
                break;
            }
            case 1:
            {
                Model = array[1].Trim().RemoveAtLineEndings();
                break;
            }
            case 2:
            {
                IMSI = array[1].Trim().RemoveAtLineEndings();
                break;
            }
            case 3:
            {
                ICCID = array[1].Trim().RemoveAtLineEndings();
                break;
            }
            default:
            {
                throw new ArgumentException($"Failed to handle index {CommandIndex}.");
                break;
            }
        }
    }
}