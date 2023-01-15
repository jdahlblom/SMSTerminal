using System.Text;
using NLog;

namespace SMSTerminal.PDU;

/// <summary>
/// Parses lists of SMS, single SMS separated from CSMS.
/// CSMS are concatenated if possible, if not the fragments
/// are stored for later use if the rest of the CSMS arrives.
/// </summary>
internal class PDUConcatenation
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    /// <summary>
    /// All SMS will go through here regardless of SMS or CSMS.
    /// Sort messages into complete SMS and fragmented CSMS.
    /// Fragmented CSMS are parts of a concatenated SMS where
    /// all parts have not yet arrived.
    /// </summary>
    public void SortMessages(List<PDUMessage> pduModemMessages,
        ref List<PDUMessage> completeMessages,
        ref List<PDUMessage> fragmentCSMSMessages)
    {

        if (pduModemMessages == null || pduModemMessages.Count == 0)
        {
            return;
        }

        /*
         * Sort messages.
         * Single message SMS => singleMessages
         * CSMS that are complete => completeMessages
         * Fragment of CSMS => fragmentCMCList
         *
         * Purge fragmentCSMSList of old entries that are likely to be orphans.
         */
        //Single SMS are removed and added to the completeMessages list before proceeding with the CSMS.
        completeMessages.AddRange(pduModemMessages.FindAll(o => o.IsCMS == false));
        pduModemMessages.RemoveAll(o => o.IsCMS == false);

        if (pduModemMessages.Count > 0)
        {
            SortCSMS(pduModemMessages, ref completeMessages, ref fragmentCSMSMessages);
        }
        Logger.Debug("PDU parse finished => Complete messages : {0}. Fragmented messages = {1}", completeMessages.Count, fragmentCSMSMessages.Count);
    }

    /// <summary>
    /// Neither list can contain single SMS.
    /// Concatenates CSMS and those that can't be concatenated are
    /// added to fragmentCSMSMessages.
    /// </summary>
    private void SortCSMS(List<PDUMessage> pduModemMessages,
        ref List<PDUMessage> completeMessages,
        ref List<PDUMessage> fragmentCSMSMessages)
    {
        /*
         * CSMS that are complete => completeMessages
         * Fragment of CSMS => fragmentCMCList
         * Purge fragmentCSMSList of old entries that are likely to be orphans.
         */
        /*
         * We have two lists, a fresh one and earlier collected partial CSMS.
         * We go through them both as the fresh list may contain
         * the missing CSMS parts.
         * Neither list contains single SMS.
         */
        pduModemMessages.AddRange(fragmentCSMSMessages);

        //All message refs that have been processed are stored here for later use
        var messageRefsToDelete = new List<int>();

        foreach (var pduModemMessage in pduModemMessages)
        {
            if (messageRefsToDelete.Exists(o => o == pduModemMessage.MessageReference))
            {
                //We have processed this already.
                continue;
            }

            //Count all CSMS that has this message reference
            var count = pduModemMessages.FindAll(o =>
                o.MessageReference == pduModemMessage.MessageReference).Count;

            // "Greater than" just in case?
            if (count >= pduModemMessage.PartsTotal)
            {
                //This is message exists complete in the list
                //Get all messages with this message reference number and sort according to their part index
                var messages = pduModemMessages.FindAll(o =>
                    o.MessageReference == pduModemMessage.MessageReference).OrderBy(o => o.ThisPart);

                //Build complete message
                var stringBuilderMessage = new StringBuilder();
                var stringBuilderRawMessage = new StringBuilder();
                foreach (var pduMessage in messages)
                {
                    stringBuilderMessage.Append(pduMessage.Message);
                    stringBuilderRawMessage.Append(pduMessage.RawMessage);
                }

                //We will use the first message and concatenate the rest of the CSMS to this
                var finalMessage = messages.First();
                //We need references to all memory slots so that the SMS can be deleted from TA
                messages.ToList().ForEach(o => finalMessage.AddMemorySlots(o.MemorySlots));

                finalMessage.Message = stringBuilderMessage.ToString();
                finalMessage.RawMessage = stringBuilderRawMessage.ToString();
                finalMessage.HasBeenConcatenated = true;
                completeMessages.Add(finalMessage);
                messageRefsToDelete.Add(finalMessage.MessageReference);
            }
            else
            {
                //This CSMS is currently an orphan and will be added to partialCSMSList if not already there
                if (fragmentCSMSMessages.FindAll(o =>
                        o.MessageReference == pduModemMessage.MessageReference &&
                        o.ThisPart == pduModemMessage.ThisPart).Count == 0)
                {
                    fragmentCSMSMessages.Add(pduModemMessage);
                }
            }
        }
        //Remove all CSMS that has been processed
        var processedCSMS = fragmentCSMSMessages.RemoveAll(o => messageRefsToDelete.Exists(x => x == o.MessageReference));
        //Last, remove all partial messages that have expired.
        var expiredCSMS = fragmentCSMSMessages.RemoveAll(o => o.HasExpired());
        Logger.Info("Removed processed CSMS : {0}. Removed expired CSMS : {1}.", processedCSMS, expiredCSMS);
    }
}