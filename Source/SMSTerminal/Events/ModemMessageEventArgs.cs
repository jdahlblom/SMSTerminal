using System.Text;
using SMSTerminal.General;

namespace SMSTerminal.Events;

public class ModemInternalEventArgs
{
    internal string ModemId { get; init; }
    internal ModemResultEnum ModemResult { get; init; } = ModemResultEnum.None;
    internal ModemDataClassEnum ModemMessageClass { get; init; }
    internal string Data { get; init; }

    internal string LogString()
    {
        var result = new StringBuilder();
        result.AppendLine($"Status = {ModemResult}");

        result.AppendLine($"Class = {ModemMessageClass}");

        result.AppendLine($"Data = ->{Data}<-");

        return result.ToString();
    }

}