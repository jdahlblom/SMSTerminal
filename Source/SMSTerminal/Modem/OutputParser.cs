using System.Text;
using System.Text.RegularExpressions;
using NLog;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Modem
{
    internal class OutputParser : IOutputParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Modem _modem;

        public OutputParser(Modem modem)
        {
            _modem = modem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modemOutput"></param>
        /// <returns>True if all modemOutput was complete</returns>
        public async Task<bool> ParseModemOutput(string modemOutput)
        {
            /*
             * Getting different results from modem, e.g. \r\nOK or \rOK
             */
            modemOutput = modemOutput.Replace('\n', '\r');

            /*
             Modem sometimes outputting several information parts in one go.
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
                    Logger.Debug($"Read this from modem:\n\n->{modemOutput}<-");
                    ModemData modemData = null;
                    try
                    {
                        modemData = new ModemData(outputBuffer.ToString());
                        if (modemData.ModemDataClass == ModemDataClassEnum.NewSMSWaiting)
                        {
                            ModemEventManager.ModemMessageEvent(this, _modem.ModemId, modemData.ModemResult, modemData.ModemDataClass, modemData.Data);
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
                        return false;
                    }

                    outputBuffer.Clear();
                }
            }

            return string.IsNullOrEmpty(outputBuffer.ToString().RemoveAtLineEndings());
        }
    }
}
