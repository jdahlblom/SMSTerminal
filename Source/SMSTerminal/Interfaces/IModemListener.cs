using SMSTerminal.Events;

namespace SMSTerminal.Interfaces;

/// <summary>
/// Anyone wanting to listen to event from the modem must implement this.
/// Contains information about errors, communication.
/// </summary>
public interface IModemListener
{
    public void ModemEvent(object sender, ModemEventArgs e);
}