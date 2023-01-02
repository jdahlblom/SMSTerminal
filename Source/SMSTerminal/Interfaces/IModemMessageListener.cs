using SMSTerminal.Events;

namespace SMSTerminal.Interfaces
{
    internal interface IModemMessageListener
    {
        void Dispose();
        internal void ModemMessageEvent(object sender, ModemMessageEventArgs e);
    }
}
