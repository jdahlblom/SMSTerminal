using SMSTerminal.Events;

namespace SMSTerminal.Interfaces
{
    /// <summary>
    /// Used for events of more of an internal usage.
    /// </summary>
    internal interface IModemInternalListener
    {
        internal void ModemInternalEvent(object sender, ModemInternalEventArgs e);
    }
}
