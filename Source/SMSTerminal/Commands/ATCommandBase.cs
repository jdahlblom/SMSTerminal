using NLog;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands;

internal abstract class ATCommandBase : ICommand
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public string CommandType { get; protected init; }
    protected readonly List<ATCommand> ATCommandsList = new();
    protected IModem Modem { get; set; }
    public ATCommand CurrentATCommand => ATCommandsList[CommandIndex];
    protected bool HasNextATCommand => ATCommandsList.Count - 1 > CommandIndex;

    /// <summary>
    /// Returns next AT command, if none exists it returns null.
    /// </summary>
    /// <returns></returns>
    public ATCommand NextATCommand()
    {
        if (ATCommandsList.Count > CommandIndex - 1)
        {
            return ATCommandsList[++CommandIndex];
        }
        return null;
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
            ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ATCommandsList[CommandIndex].ATCommandString} was successful . {Result} \n\n\n\n{ATCommandsList[CommandIndex].ModemData.Data}", ModemEventType.ModemComms, Modem.ModemId, ModemResultEnum.Ok);
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
            if (modemData.HasCError)
            {
                ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ATCommandsList[CommandIndex].ATCommandString} resulted in {modemData.CErrorMessage}\n\n\n\n{ATCommandsList[CommandIndex].ModemData.Data}", ModemEventType.ModemComms, Modem.ModemId, modemData.ModemResult);
            }
            else
            {
                ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ATCommandsList[CommandIndex].ATCommandString}  resulted in error. {Result}\n\n\n\n{ATCommandsList[CommandIndex].ModemData.Data}", ModemEventType.ModemComms, Modem.ModemId, modemData.ModemResult);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }
}