using NLog;

namespace SMSTerminal.General;

public enum SignalType
{
    ModemWriting,
    ReadingModem,
    ExecutingCommand,
    SendingSMS,
    ReadingSMS
}

/// <summary>
/// Thread safe signals used to e.g. not execute command if command is already being executed.
/// </summary>
internal class Signals
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private  long _writingToModem;
    private  long _readingFromModem;
    private  long _executingCommand;
    private long _sendingSMS;
    private long _readingSMS;

    internal  bool IsActive(SignalType signalType, string location)
    {
        Logger.Debug($"[{location}]  {signalType} ====>    " + Interlocked.Read(ref GetSignal(signalType)));
        return Interlocked.Read(ref GetSignal(signalType)) > 0;
    }

    internal  long GetInstanceCount(SignalType signalType)
    {
        return Interlocked.Read(ref GetSignal(signalType));
    }

    internal  void SetStarted(SignalType signalType)
    {
        Interlocked.Increment(ref GetSignal(signalType));
    }

    internal  void SetEnded(SignalType signalType)
    {
        Interlocked.Decrement(ref GetSignal(signalType));
    }

    private  ref long GetSignal(SignalType signalType)
    {
        switch (signalType)
        {
            case SignalType.ModemWriting: return ref _writingToModem;
            case SignalType.ReadingModem: return ref _readingFromModem;
            case SignalType.ExecutingCommand: return ref _executingCommand;
            case SignalType.SendingSMS: return ref _sendingSMS;
            case SignalType.ReadingSMS: return ref _readingSMS;
            default: throw new NotImplementedException();
        };
    }
}