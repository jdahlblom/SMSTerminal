using NLog;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    internal abstract class CommandBase : ICommand
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public string CommandType { get; protected init; }
        protected readonly List<Command> ModemCommandsList = new();
        protected IModem Modem { get; set; }
        public void SetModem(IModem modem)
        {
            Modem = modem;
        }
        public List<Command> ModemCommands => ModemCommandsList;
        public Command CurrentATCommand => ModemCommandsList[CommandIndex];
        protected bool HasNextCommand => ModemCommandsList.Count - 1 > CommandIndex;

        public Command NextATCommand()
        {
            if (ModemCommandsList.Count > CommandIndex - 1)
            {
                return ModemCommandsList[++CommandIndex];
            }
            return null;
        }
        
        public ModemResultEnum Result { get; set; }
        public abstract CommandProgress Process(ModemData modemData);

        protected int CommandIndex = 0;
        
        protected void SetModemDataForCurrentCommand(ModemData modemData)
        {
            ModemCommandsList[CommandIndex].ModemData = modemData;
            Result = modemData.ModemResult;
        }

        protected void SendEvent(string message, ModemEventType modemEventType = ModemEventType.ModemComms)
        {
            ModemEventManager.ModemEvent(this, Modem.ModemId, message, modemEventType, Modem.ModemId, Result);
        }

        protected void SendResultEvent()
        {
            var modemData = ModemCommandsList[CommandIndex].ModemData;
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
                ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ModemCommandsList[CommandIndex].CommandString} was successful . {Result}", ModemEventType.ModemComms, Modem.ModemId, ModemResultEnum.Ok);
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
                var modemData = ModemCommandsList[CommandIndex].ModemData;
                if (modemData.HasCError)
                {
                    ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ModemCommandsList[CommandIndex].CommandString} resulted in {modemData.CErrorMessage}", ModemEventType.ModemComms, Modem.ModemId, modemData.ModemResult);
                }
                else
                {
                    ModemEventManager.ModemEvent(this, Modem.ModemId, $"{CommandType} => {ModemCommandsList[CommandIndex].CommandString}  resulted in error. {Result}", ModemEventType.ModemComms, Modem.ModemId, modemData.ModemResult);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
