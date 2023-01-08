using NLog;
using SMSTerminal.Events;

namespace SMSTerminal.General
{
    public enum ModemDataClassEnum
    {
        None,
        NewSMSWaiting,
        UnknownModemData
    }

    /// <summary>
    /// Contains modem output.
    /// </summary>
    internal class ModemData
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal ModemResultEnum ModemResult { get; set; } = ModemResultEnum.None;
        internal ModemDataClassEnum ModemDataClass { get; set; } = ModemDataClassEnum.None;
        internal string Data { get; set; }
        internal bool HasCError => !string.IsNullOrEmpty(CErrorMessage);
        internal string CErrorMessage { get; set; }

        public ModemData(string data)
        {
            try
            {
                Data = data;
                var result = ErrorCodes.HasCError(data);
                if (result.Item1)
                {
                    CErrorMessage = result.Item2;
                }

                Classify();
                AddModemDataStatus();
            }
            catch (Exception e)
            {
                Logger.Error("ModemData Constructor\n{0}", e.DecodeException());
            }
        }

        public string ErrorMessage()
        {
            return !HasCError ? Data : CErrorMessage;
        }

        private void Classify()
        {
            try
            {
                /*
                 * New SMS in modem memory (unsolicited)
                 */
                if (Data.Contains(ATMarkers.NewSMSArrivedMT) || Data.Contains(ATMarkers.NewSMSArrivedSM))
                {
                    ModemDataClass = ModemDataClassEnum.NewSMSWaiting;
                }
            }
            catch (Exception e)
            {
                Logger.Error("ModemData Exception Data:\n->{0}<-\nException :\n{1}", Data, e);
                throw;
            }
        }

        public bool HasError => ModemResult.ContainsError();

        private void AddModemDataStatus()
        {
            if (Data.Contains(ATMarkers.OkReply))
            {
                ModemResult = ModemResultEnum.Ok;
                return;
            }
            if (Data.Contains(ATMarkers.CMEErrorReply))
            {
                ModemResult = ModemResultEnum.CMEError;
                return;
            }
            if (Data.Contains(ATMarkers.CMSErrorReply))
            {
                ModemResult = ModemResultEnum.CMSError;
                return;
            }
            if (Data.Contains(ATMarkers.ErrorReply))
            {
                ModemResult = ModemResultEnum.Error;
                return;
            }
        }
        
        public override string ToString()
        {
            return $"ModemDataClass = {ModemDataClass}\n" +
                   $"HasCError = {HasCError}\n" +
                   $"CErrorMessage = {CErrorMessage}\n" +
                   $"ModemDataStatus = {ModemResult}" +
                   $"Data ->{Data}<-";
        }
    }
}
