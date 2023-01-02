using SMSTerminal.Events;

namespace SMSTerminal.Interfaces
{
    public interface INewSMSListener
    {
        public void NewSMSEvent(object sender, SMSReceivedEventArgs e);
    }
}
