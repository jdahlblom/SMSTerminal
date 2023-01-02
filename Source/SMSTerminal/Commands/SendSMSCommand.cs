using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.Commands
{
    internal class SendSMSCommand : CommandBase
    {
        private readonly OutgoingSms _outgoingSms;

        public SendSMSCommand(IModem modem, OutgoingSms outgoingSms)
        {
            Modem = modem;
            _outgoingSms = outgoingSms;
            CommandType = "[Send SMS Command]";
            FillCommandList();
        }

        public override CommandProgress Process(ModemData modemData)
        {
            try
            {
                if (!modemData.Data.Contains(ModemCommandsList[CommandIndex].CommandString))
                {
                    return CommandProgress.NotExpectedDataReply;
                }
                SetModemDataForCurrentCommand(modemData);

                Thread.Sleep(ModemTimings.MS200);

                if (modemData.HasError)
                {
                    SendResultEvent();
                    return CommandProgress.Error;
                }

                //mod = 0 means it contains the AT command
                if (CommandIndex % 2 == 0 && modemData.Data.Contains(ATMarkers.ReadyPrompt))
                {
                    SendResultEvent();
                    return CommandProgress.NextCommand;
                }

                //mod = 1 means it contains the PDU
                if (CommandIndex % 2 == 1)
                {
                    SendResultEvent();
                    return HasNextCommand ? CommandProgress.NextCommand : CommandProgress.Finished;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return CommandProgress.Error;
            }

            return CommandProgress.Finished;
        }

        private void FillCommandList()
        {
            if (string.IsNullOrEmpty(_outgoingSms.Message))
            {
                throw new Exception("OutgoingSMS has no message. Cannot send.");
            }
            if (!_outgoingSms.ReceiverTelephone.IsValidTph())
            {
                throw new Exception("OutgoingSMS has an invalid recipient telephone number. Cannot send.");
            }

            var pduArray = new PDUEncoder(_outgoingSms.ReceiverTelephone,
                _outgoingSms.Message,
                _outgoingSms.SMSEncoding,
                ModemTimings.SMSDaysValid,
                _outgoingSms.RequestStatusReport,
                true).MakePDU();
            _outgoingSms.SetUsedPDU(pduArray);

            foreach (var pdu in pduArray)
            {
                //Add AT command to prepare modem for SMS data
                ModemCommandsList.Add(new Command(ATCommands.ATSendSmsPDU + (pdu.Length - 2) / 2, ATCommands.ATEndPart));
                //Add the PDU that will be sent (SMS data)
                ModemCommandsList.Add(new Command(pdu, ATCommands.ATCommandCtrlZ.ToString()));
            }
        }
    }
}
