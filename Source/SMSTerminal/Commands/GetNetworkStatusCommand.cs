using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands
{
    internal class GetNetworkStatusCommand : CommandBase
    {
        public GetNetworkStatusCommand(IModem modem)
        {
            Modem = modem;
            CommandType = "[Get Network Status Command]";
            var command = new Command(ATCommands.ATNetworkStatusRequestCommand, ATCommands.ATEndPart);
            ModemCommandsList.Add(command);
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
                
                if (modemData.HasError)
                {
                    return CommandProgress.Error;
                }

                var networkStatus = ParseNetworkStatus(modemData.Data);
                SendEvent(networkStatus.ToString(), ModemEventType.NetworkStatus);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return CommandProgress.Error;
            }

            return CommandProgress.Finished;
        }

        private GsmNetworkRegistrationStatus ParseNetworkStatus(string modemReply)
        {
            var result = GsmNetworkRegistrationStatus.Unknown;
            
            try
            {
                var status = int.Parse(modemReply.Substring(modemReply.IndexOf(",", StringComparison.Ordinal) + 1, 1));
                result = (GsmNetworkRegistrationStatus)status;
            }
            catch (Exception e)
            {
                Logger.Error("ParseNetworkStatus : " + e.DecodeException());
            }

            return result;
        }
    }
}
