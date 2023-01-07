using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;
using SMSTerminal.SMSMessages;

namespace SMSTerminal.Commands
{
    public enum SMSReadStatus
    {
        Read,
        Unread
    }

    /// <summary>
    /// Reads and parses SMS from the modem.
    /// </summary>
    internal class ATReadSMSCommand : ATCommandBase
    {
        private readonly string _commandString;


        public ATReadSMSCommand(IModem modem, SMSReadStatus smsReadStatus)
        {
            Modem = modem;
            CommandType = $"[Read [{smsReadStatus}] SMS Command]";
            _commandString = smsReadStatus == SMSReadStatus.Read
                ? General.ATCommands.ATReadReadSms
                : General.ATCommands.ATReadUnreadSms;
            ATCommandsList.Add(new ATCommand(_commandString, General.ATCommands.ATEndPart));
        }

        public override CommandProgress Process(ModemData modemData)
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

                if (modemData.Data.Contains(_commandString))
                {
                    var readMessages = PDUMessageParser.ParseRawModemOutput(modemData.Data);
                    Logger.Debug("PDU returned {0} messages.", readMessages.Count);
                    if (readMessages.Count == 0)
                    {
                        return CommandProgress.Finished;
                    }

                    /*
                     * Send event about new SMS and also add delete from memory command
                     */
                    foreach (var modemMessage in readMessages)
                    {
                        ModemEventManager.NewSMSEvent(this, IncomingSms.Convert(modemMessage), modemMessage);
                        if (!Modem.GsmModemConfig.DeleteSMSFromModemWhenRead) break;
                        foreach (var i in modemMessage.MemorySlots)
                        {
                            ATCommandsList.Add(new ATCommand(General.ATCommands.ATDeleteSmsAtMemorySlot + i, General.ATCommands.ATEndPart, i));
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
                            ATCommandsList.Add(new ATCommand(General.ATCommands.ATDeleteSmsAtMemorySlot + i, General.ATCommands.ATEndPart, i, "Fragment"));
                            //fragmentCSMSMessage.DeletedFromTA = true;
                        }
                    }

                    return CommandProgress.NextCommand; //Start doing the delete commands
                }
                else if (modemData.Data.Contains(General.ATCommands.ATDeleteSmsAtMemorySlot))
                {
                    /*
                     * A SMS that belongs to a CSMS but hasn't yet been assembled because all parts haven't yet arrived.
                     */
                    if(ATCommandsList[CommandIndex].StringHolder == "Fragment")
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
}
