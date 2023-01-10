using SMSTerminal.General;
using SMSTerminal.Interfaces;

namespace SMSTerminal.Commands;

/// <summary>
/// Handles PIN operations, checks status and applies PIN1 if necessary.
/// </summary>
internal class ATSetPINCommand : ATCommandBase
{
    private string PIN1Code { get; }
    public bool PINCodeSet { get; private set; }

    public ATSetPINCommand(IModem modem, string pinCode)
    {
        Modem = modem;
        PIN1Code = pinCode;
        CommandType = "[Set PIN Command]";
        var command = new ATCommand(ATCommands.ATPINStatusQueryCommand, ATCommands.ATEndPart);
        var command2 = new ATCommand(ATCommands.ATPINAuthCommand + PIN1Code, ATCommands.ATEndPart);
        ATCommandsList.Add(command);
        ATCommandsList.Add(command2);
    }

    public override async Task<CommandProgress> Process(ModemData modemData)
    {
        try
        {
            //Give modem some breathing space. SMS is slow communication.
            await Task.Delay(ModemTimings.MS100);

            if (!modemData.Data.Contains(ATCommandsList[CommandIndex].ATCommandString))
            {
                return CommandProgress.NotExpectedDataReply;
            }
            SetModemDataForCurrentCommand(modemData);

            switch (CommandIndex)
            {
                case 0:
                {
                    var result = ParseAuthenticationStatus(modemData.Data);
                    switch (result)
                    {
                        case PINAuthenticationStatus.AwaitingPIN1:
                        {
                            SendResultEvent();
                            return CommandProgress.NextCommand;
                        }
                        case PINAuthenticationStatus.StatusOK:
                        {
                            SendResultEvent();
                            return CommandProgress.Finished;
                        }
                        default:
                        {
                            SendEvent($"Failed to determine SIM PIN status. [HaltForPINError]. {result}");
                            Modem.HaltForPINError = true;
                            return CommandProgress.Error;
                        }
                    }
                }
                case 1:
                {
                    SendResultEvent();
                    if (modemData.HasError)
                    {
                        SendEvent($"Error applying PIN code. [HaltForPINError]. {modemData.ErrorMessage()}");
                        Modem.HaltForPINError = true;
                        return CommandProgress.Error;
                    }

                    PINCodeSet = true;
                    return CommandProgress.Finished;
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

    private static PINAuthenticationStatus ParseAuthenticationStatus(string modemReply)
    {
        if (string.IsNullOrEmpty(modemReply))
        {
            return PINAuthenticationStatus.AuthenticationStatusUnknown;
        }
        if (modemReply.Contains("+CPIN: READY"))
        {
            return PINAuthenticationStatus.StatusOK;
        }
        if (modemReply.Contains("+CPIN: SIM PIN"))
        {
            return PINAuthenticationStatus.AwaitingPIN1;
        }
        if (modemReply.Contains("+CPIN: SIM PUK"))
        {
            return PINAuthenticationStatus.AwaitingPUK1;
        }
        if (modemReply.Contains("+CPIN: SIM PIN2"))
        {
            return PINAuthenticationStatus.AwaitingPIN2;
        }
        if (modemReply.Contains("+CPIN: SIM PUK2"))
        {
            return PINAuthenticationStatus.AwaitingPUK2;
        }
        if (modemReply.Contains("+CPIN: PH-SIM PIN"))
        {
            return PINAuthenticationStatus.AwaitingPhoneToSIMPIN;
        }
        if (modemReply.Contains("+CPIN: PH-SIM PUK"))
        {
            return PINAuthenticationStatus.AwaitingMasterPhoneCodePUK;
        }
        if (modemReply.Contains("+CPIN: PH-FSIM PIN"))
        {
            return PINAuthenticationStatus.AwaitingPhoneToVerifyFirstSIMPIN;
        }
        if (modemReply.Contains("+CPIN: PH-FSIM PUK"))
        {
            return PINAuthenticationStatus.AwaitingPhoneToVerifyFirstSIMPUK;
        }
        if (modemReply.Contains("+CPIN: PH-NET PUK"))
        {
            return PINAuthenticationStatus.AwaitingPhoneToNetworkPersonalisationPUK;
        }
        if (modemReply.Contains("+CPIN: PH-NS PIN"))
        {
            return PINAuthenticationStatus.AwaitingNetworkSubsetPersonalisationPIN;
        }
        if (modemReply.Contains("+CPIN: PH-NS PUK"))
        {
            return PINAuthenticationStatus.AwaitingNetworkSubsetPersonalisationPUK;
        }
        if (modemReply.Contains("+CPIN: PH-SP PIN"))
        {
            return PINAuthenticationStatus.AwaitingServiceProviderPersonalisationPIN;
        }
        if (modemReply.Contains("+CPIN: PH-SP PUK"))
        {
            return PINAuthenticationStatus.AwaitingServiceProviderPersonalisationPUK;
        }
        if (modemReply.Contains("+CPIN: PH-C PIN"))
        {
            return PINAuthenticationStatus.AwaitingCorporatePersonalisationPIN;
        }
        if (modemReply.Contains("+CPIN: PH-C PUK"))
        {
            return PINAuthenticationStatus.AwaitingCorporatePersonalisationPUK;
        }
        if (modemReply.Contains("+CME ERROR: SIM blocked"))
        {
            return PINAuthenticationStatus.SIMBlocked;
        }
        if (modemReply.Contains("+CME ERROR: SIM busy"))
        {
            return PINAuthenticationStatus.SIMBusy;
        }
        return PINAuthenticationStatus.AuthenticationStatusUnknown;
    }
}