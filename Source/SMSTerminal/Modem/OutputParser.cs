using System.Text;
using System.Text.RegularExpressions;
using NLog;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Modem;

/// <summary>
/// Parses modem output, checks for (end of message) markers and creates
/// ModemData when marker found. The rest of the message if not complete will be
/// left as is by calling class until it too is complete.
/// Modem outputs in bursts and can contain
/// a) single complete message
/// b) single incomplete message
/// c) several not related messages where last
/// message can be complete or incomplete.
/// </summary>
internal class OutputParser : IOutputParser
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Modem _modem;

    public OutputParser(Modem modem)
    {
        _modem = modem;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modemOutput"></param>
    /// <returns>True if all modemOutput was complete</returns>
    public async Task<string> ParseModemOutput(string modemOutput)
    {
        if (string.IsNullOrEmpty(modemOutput))
        {
            return "";
        }

        /*
         * Getting different results from modem, => \r\nOK or \rOK
         */
        modemOutput = modemOutput.Replace('\n', '\r');

        /*
         Modem sometimes outputting several information parts (messages) in one go.
         Here [Send SMS Report] (CMGS) and new SMS in memory indicator (CMTI) :

            <PDU>
            +CMGS: 0            
            OK
        
            +CMTI: "SM",14
         */
        var outputLines = Regex.Split(modemOutput, @"(?<=[\r])");
        var outputBuffer = new StringBuilder();
        foreach (var line in outputLines)
        {
            outputBuffer.Append(line);
            if (outputBuffer.ToString().ContainsOutputEndMarker())
            {
                /*
                 * We have found the end of the modem output ("message"), separate this and process it
                 * further. Remove the corresponding string ("message") from the modemOutput as
                 * processed messages shouldn't be returned. modemOutput can contain incomplete (still incoming)
                 * modem output that will be processed later on.
                 */
                Logger.Debug($"Read this message from modem ({outputBuffer.Length}) chars:\n\n->{outputBuffer}<-");
                modemOutput = modemOutput[outputBuffer.Length..];
                Logger.Debug($"Rest of buffer is ({modemOutput.Length}) chars:\n\n->{modemOutput}<-");
                ModemData modemData = null;
                try
                {
                    modemData = new ModemData(outputBuffer.ToString());
                    if (modemData.ModemDataClass == ModemDataClassEnum.NewSMSWaiting)
                    {
                        ModemEventManager.ModemInternalEvent(this, _modem.ModemId, modemData.ModemResult, modemData.ModemDataClass, modemData.Data);
                    }
                    else
                    {
                        var cts = new CancellationTokenSource(ModemTimings.MS100);
                        await _modem.ModemDataChannel.Writer.WriteAsync(modemData, cts.Token);
                    }
                }
                catch (Exception exception)
                {
                    var message = modemData == null ? "" : modemData.ToString();
                    Logger.Error("Failed creating ModemData.\n{0}\n\n{1}", message, exception);
                }

                outputBuffer.Clear();
            }
        }

        return modemOutput.Trim().RemoveAtLineEndings().Length == 0 ? "" : modemOutput;
    }
}