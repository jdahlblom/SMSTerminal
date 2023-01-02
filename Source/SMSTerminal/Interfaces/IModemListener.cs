using SMSTerminal.Events;

namespace SMSTerminal.Interfaces
{
    public interface IModemListener
    {
        public void ModemEvent(object sender, ModemEventArgs e);
    }
}
