using SMSTerminal.Commands;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Modem;

namespace SMSTerminal.Interfaces;

/// <summary>
/// Interface for commands that can be executed by a IModem.
/// </summary>
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
        
    /// <summary>
    /// Retrieves AT command command index is pointing at.
    /// Calling NextATCommand will increase command index.
    /// </summary>
    ATCommandLine CurrentATCommand { get; }

    /// <summary>
    /// Once this has been called also CurrentATCommand
    /// will point to the same ATCommand. Internal Command Index is
    /// increased by 1.
    /// </summary>
    /// <returns>next AT command</returns>
    ATCommandLine NextATCommand();

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
    Task<CommandProgress> Process(ModemData modemData);
}