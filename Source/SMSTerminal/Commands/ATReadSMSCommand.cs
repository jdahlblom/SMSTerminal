﻿using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;
using SMSTerminal.PDU;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.Commands;

public enum SMSReadStatus
{
    Read,
    Unread
}

/// <summary>
/// Reads and parses SMS from the modem.
/// </summary>
internal class ATReadSMSCommand : ATCommand
{
    private readonly string _readCommandString;


    public ATReadSMSCommand(IModem modem, SMSReadStatus smsReadStatus)
    {
        Modem = modem;
        CommandType = $"[Read [{smsReadStatus}] SMS Command]";
        _readCommandString = smsReadStatus == SMSReadStatus.Read
            ? ATCommands.ATReadReadSms
            : ATCommands.ATReadUnreadSms;
        ATCommandsList.Add(new ATCommandLine(_readCommandString, ATCommands.ATEndPart));
    }

    public override async Task<CommandProgress> Process(ModemData modemData)
    {
        try
        {
            //Give modem some breathing space. SMS is slow communication.
            await Task.Delay(ModemTimings.MS200);

            if (!modemData.Data.Contains(ATCommandsList[CommandIndex].ATCommandString))
            {
                return CommandProgress.NotExpectedDataReply;
            }
            SetModemDataForCurrentCommand(modemData);
            SendResultEvent();

            if (modemData.HasError)
            {
                return CommandProgress.Error;
            }

            if (modemData.Data.Contains(_readCommandString))
            {
                var readCompleteMessages = PDUMessageParser.ParseRawModemOutput(modemData.Data);
                Logger.Debug("PDU returned {0} complete messages, {1} fragmented messages exist.",
                    readCompleteMessages.Count, PDUMessageParser.FragmentCSMSMessages.Count);
                
                if (readCompleteMessages.Count > 0)
                {
                    /*
                     * Send event about new SMS and also add delete from memory command
                     */
                    foreach (var modemMessage in readCompleteMessages)
                    {
                        var incomingSMS = IncomingSms.Convert(modemMessage);
                        if (incomingSMS.IsStatusReport)
                        {
                            /*
                             * Must confirm that report has been received.
                             */
                            ATCommandsList.Add(new ATCommandLine(ATCommands.ATSMSStatusReportACK, ATCommands.ATEndPart)); 
                        }
                        ModemEventManager.NewSMSEvent(this, incomingSMS);

                        if (!Modem.GsmModemConfig.DeleteSMSFromModemWhenRead) break;
                        foreach (var i in modemMessage.MemorySlots)
                        {
                            ATCommandsList.Add(new ATCommandLine(ATCommands.ATDeleteSmsAtMemorySlot + i, ATCommands.ATEndPart, i));
                        }
                    }
                }

                /*
                 * We need to delete fragmented CSMS from TA which aren't included in the above list.
                 * They are still kept in a list by PDUMessageParser so that they can be concatenated later on.
                 */
                foreach (var fragmentCSMSMessage in PDUMessageParser.FragmentCSMSMessages)
                {
                    if (!Modem.GsmModemConfig.DeleteSMSFromModemWhenRead) break;
                    if (fragmentCSMSMessage.DeletedFromTA) continue;
                    foreach (var i in fragmentCSMSMessage.MemorySlots)
                    {
                        ATCommandsList.Add(new ATCommandLine(ATCommands.ATDeleteSmsAtMemorySlot + i, ATCommands.ATEndPart, i, "Fragment"));
                        //fragmentCSMSMessage.DeletedFromTA = true;
                    }
                }

                return HasNextATCommand ? CommandProgress.NextCommand : CommandProgress.Finished;
            }


            if (modemData.Data.Contains(ATCommands.ATDeleteSmsAtMemorySlot))
            {
                /*
                 * A SMS that belongs to a CSMS but hasn't yet been assembled because all parts haven't yet arrived.
                 */
                if (ATCommandsList[CommandIndex].StringHolder == "Fragment")
                {
                    PDUMessageParser.MarkFragmentDeletedTA(ATCommandsList[CommandIndex].NumberHolder);
                }
                SendEvent($"SMS deleted from slot {ATCommandsList[CommandIndex].NumberHolder}.");
                if (HasNextATCommand)
                {
                    return CommandProgress.NextCommand;
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return CommandProgress.Error;
        }

        return CommandProgress.Finished;
    }

}