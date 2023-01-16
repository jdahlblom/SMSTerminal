using NLog;
using SMSTerminal.Events;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

internal abstract class ATCommand : ICommand
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public string CommandType { get; protected init; }
    protected readonly List<ATCommandLine> ATCommandsList = new();
    protected IModem Modem { get; set; }
    public ATCommandLine CurrentATCommand => ATCommandsList[CommandIndex];
    protected bool HasNextATCommand => ATCommandsList.Count - 1 > CommandIndex;

    /// <summary>
    /// Returns next AT command, if none exists it returns null.
    /// </summary>
    /// <returns></returns>
    public ATCommandLine NextATCommand()
    {
        if (ATCommandsList.Count > CommandIndex - 1)
        {
            return ATCommandsList[++CommandIndex];
        }
        return null;
    }

    public string GetErrors()
    {
        var result = "";
        foreach (var atCommand in ATCommandsList.Where(atCommand => atCommand.ModemData != null))
        {
            if (atCommand.ModemData.HasError)
            {
                result += $"{atCommand.ModemData.ModemResult}\n";
            }
            if (atCommand.ModemData.HasCError)
            {
                result += $"{atCommand.ModemData.CErrorMessage}";
            }
        }

        return result;
    }

    /// <summary>
    /// Contains result of last executed AT command.
    /// </summary>
    public ModemResultEnum Result { get; set; }

    /// <summary>
    /// Processes the reply from the modem. Result indicates
    /// whether to continue executing AT commands or to finish.
    /// </summary>
    /// <param name="modemData"></param>
    /// <returns></returns>
    public abstract Task<CommandProgress> Process(ModemData modemData);

    protected int CommandIndex = 0;
        
    /// <summary>
    /// Stores the ModemData for current AT command.
    /// </summary>
    /// <param name="modemData"></param>
    protected void SetModemDataForCurrentCommand(ModemData modemData)
    {
        ATCommandsList[CommandIndex].ModemData = modemData;
        Result = modemData.ModemResult;
    }

    protected void SendEvent(string message, ModemEventType modemEventType = ModemEventType.ModemComms)
    {
        ModemEventManager.ModemEvent(this, Modem.ModemId, message, modemEventType, Modem.ModemId, Result);
    }

    protected void SendResultEvent()
    {
        var modemData = ATCommandsList[CommandIndex].ModemData;
        if (modemData.HasError)
        {
            SendErrorEvent();
        }
        else
        {
            SendOKEvent();
        }
    }

    protected void SendOKEvent()
    {
        try
        {
            ModemEventManager.ATCommandEvent(this,
                Modem.ModemId,
                ATCommandsList[CommandIndex].ATCommandString,
                $"Command {CommandType} was successful.",
                "",
                ModemEventType.ATCommand,
                ModemResultEnum.Ok);
                /*ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ATCommandsList[CommandIndex].ATCommandString} was successful . {Result} \n" +
                                                              $"\n----------------------------\\n\n{ATCommandsList[CommandIndex].ModemData.Data}",
                ModemEventType.ModemComms, Modem.ModemId, ModemResultEnum.Ok);*/
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    protected void SendErrorEvent()
    {
        try
        {
            var modemData = ATCommandsList[CommandIndex].ModemData;
            ModemEventManager.ATCommandEvent(this,
                Modem.ModemId,
                ATCommandsList[CommandIndex].ATCommandString,
                $"Command {CommandType} failed.",
                GetErrors(),
                ModemEventType.ATCommand,
                Result);

            if (modemData.HasCError)
            {
                //ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ATCommandsList[CommandIndex].ATCommandString} resulted in {modemData.CErrorMessage}\n----------------------------\n{ATCommandsList[CommandIndex].ModemData.Data}", ModemEventType.ModemComms, Modem.ModemId, modemData.ModemResult);
            }
            else
            {
                //ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ATCommandsList[CommandIndex].ATCommandString}  resulted in error. {Result}\n----------------------------\n{ATCommandsList[CommandIndex].ModemData.Data}", ModemEventType.ModemComms, Modem.ModemId, modemData.ModemResult);
            }
            Logger.Error($"{Modem} : Command {CommandType} failed.\nAT command = {ATCommandsList[CommandIndex].ATCommandString}\n{GetErrors()}");
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }
}