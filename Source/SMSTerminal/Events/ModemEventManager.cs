﻿using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Events;

public static class ModemEventManager
{
    /*********************************************************************************************************************/
    public delegate void ATCommandEventHandler(object sender, ATCommandEventArgs e);
    public static event ATCommandEventHandler OnATCommandEvent;

    /// <summary>
    /// Messages relating to executing AT commands
    /// </summary>
    public static void ATCommandEvent(object sender, string modemId, string atCommand, string message, string errorMessage, ModemEventType modemEventType, ModemResultEnum resultStatus)
    {
        OnATCommandEvent?.Invoke(sender, new ATCommandEventArgs
        {
            ModemId = modemId,
            ATCommand = atCommand,
            Message = message,
            ErrorMessage = errorMessage,
            EventType = modemEventType,
            ResultStatus = resultStatus
        });
    }

    public static void AttachATEventListener(IATCommandListener atCommandListener)
    {
        OnATCommandEvent += atCommandListener.ATCommandEvent;
    }

    public static void DetachATEventListener(IATCommandListener atCommandListener)
    {
        OnATCommandEvent -= atCommandListener.ATCommandEvent;
    }

    /*********************************************************************************************************************/
    public delegate void ModemEventHandler(object sender, ModemEventArgs e);
    public static event ModemEventHandler OnModemEvent;

    /// <summary>
    /// Messages relating to the communication and modem configuration
    /// </summary>
    public static void ModemEvent(object sender, string modemId, string message, ModemEventType modemEventType, string id, ModemResultEnum resultStatus)
    {
        OnModemEvent?.Invoke(sender, new ModemEventArgs { 
            ModemId = modemId,
            Message = message,
            EventType = modemEventType, 
            Id = id,
            ResultStatus = resultStatus
        });
    }

    public static void AttachModemEventListener(IModemListener modemListener)
    {
        OnModemEvent += modemListener.ModemEvent;
    }

    public static void DetachModemEventListener(IModemListener modemListener)
    {
        OnModemEvent -= modemListener.ModemEvent;
    }
        
    /*********************************************************************************************************************/
    internal delegate void ModemInternalEventHandler(object sender, ModemInternalEventArgs e);
    internal static event ModemInternalEventHandler OnModemInternalEvent;

    /// <summary>
    /// Internal use for sending events about data from modem
    /// </summary>
    internal static void ModemInternalEvent(object sender, string modemId, ModemResultEnum modemResultEnum, ModemDataClassEnum modemDataClass, string data)
    {
        OnModemInternalEvent?.Invoke(sender, new ModemInternalEventArgs { 
            ModemId = modemId,
            ModemResult = modemResultEnum,
            ModemMessageClass = modemDataClass,
            Data = data
        });
    }

    internal static void AttachModemMessageListener(IModemInternalListener modemMessageListener)
    {
        OnModemInternalEvent += modemMessageListener.ModemInternalEvent;
    }

    internal static void DetachModemMessageListener(IModemInternalListener modemMessageListener)
    {
        OnModemInternalEvent -= modemMessageListener.ModemInternalEvent;
    }

    /*********************************************************************************************************************/
    public delegate void NewSMSEventHandler(object sender, SMSReceivedEventArgs e);
    public static event NewSMSEventHandler OnNewSMSEvent;

    /// <summary>
    /// Used when new SMS has been read
    /// </summary>
    public static void NewSMSEvent(object sender, IShortMessageService shortMessageService)
    {
        OnNewSMSEvent?.Invoke(sender, new SMSReceivedEventArgs
        {
            ShortMessageService = shortMessageService
        });
    }

    public static void AttachNewSMSListener(INewSMSListener newSMSListener)
    {
        OnNewSMSEvent += newSMSListener.NewSMSEvent;
    }

    public static void DetachNewSMSListener(INewSMSListener newSMSListener)
    {
        OnNewSMSEvent -= newSMSListener.NewSMSEvent;
    }
}