using SMSTerminal.Events;

namespace SMSTerminal.Interfaces;

/// <summary>
/// Anyone wanting to listen to events from AT commands from the modem must implement this.
/// Contains information about errors, communication.
/// </summary>
public interface IATCommandListener
{
    public void ATCommandEvent(object sender, ATCommandEventArgs e);
}