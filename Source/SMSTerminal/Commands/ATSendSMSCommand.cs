using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.Commands;

/// <summary>
/// Sends SMS
/// </summary>
internal class ATSendSMSCommand : ATCommandBase
{
    private readonly OutgoingSms _outgoingSms;

    public ATSendSMSCommand(IModem modem, OutgoingSms outgoingSms)
    {
        Modem = modem;
        _outgoingSms = outgoingSms;
        CommandType = "[Send SMS Command]";
        FillCommandList();
    }

    public override async Task<CommandProgress> Process(ModemData modemData)
    {
        try
        {
            if (!modemData.Data.Contains(ATCommandsList[CommandIndex].ATCommandString))
            {
                return CommandProgress.NotExpectedDataReply;
            }
            SetModemDataForCurrentCommand(modemData);
                
            if (modemData.HasError)
            {
                SendResultEvent();
                return CommandProgress.Error;
            }
                
            //Give modem some breathing space. SMS is slow communication.
            await Task.Delay(ModemTimings.MS500);

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
                return HasNextATCommand ? CommandProgress.NextCommand : CommandProgress.Finished;
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
            ATCommandsList.Add(new ATCommand(ATCommands.ATSendSmsPDU + (pdu.Length - 2) / 2, ATCommands.ATEndPart));
            //Add the PDU that will be sent (SMS data)
            ATCommandsList.Add(new ATCommand(pdu, ATCommands.ATCommandCtrlZ.ToString()));
        }
    }
}