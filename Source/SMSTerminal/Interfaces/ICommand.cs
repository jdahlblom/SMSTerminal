using SMSTerminal.Commands;
using SMSTerminal.Events;
using SMSTerminal.General;

namespace SMSTerminal.Interfaces
{
    internal enum CommandProgress
    {
        /// <summary>
        /// If a modem response was given to a command
        /// it wasn't related to then this will be returned.
        /// </summary>
        NotExpectedDataReply,
        NextCommand,
        Error,
        Finished
    }

    internal interface ICommand
    {
        /// <summary>
        /// Output friendly string describing the command.
        /// </summary>
        string CommandType { get; }
        void SetModem(IModem modem);
        List<ATCommand> ATCommands { get; }
        ATCommand CurrentATCommand { get; }
        ATCommand NextATCommand();

        /// <summary>
        /// This is the final result of the command
        /// regardless of what earlier commands yielded.
        /// </summary>
        ModemResultEnum Result { get; }

        /// <summary>
        /// Processes the results for the current command (command last executed).
        /// This can be set by the command itself or by the modem.
        /// </summary>
        /// <param name="modemData"></param>
        /// <returns></returns>
        CommandProgress Process(ModemData modemData);
    }
}