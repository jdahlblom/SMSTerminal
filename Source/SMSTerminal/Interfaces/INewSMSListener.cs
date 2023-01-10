using SMSTerminal.Events;

namespace SMSTerminal.Interfaces
{
    /// <summary>
    /// For anyone wanting to listen to new SMS events.
    /// </summary>
    public interface INewSMSListener
    {
        public void NewSMSEvent(object sender, SMSReceivedEventArgs e);
    }
}
