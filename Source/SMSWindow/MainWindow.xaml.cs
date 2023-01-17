using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using NLog;
using SMSTerminal.Events;
using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.PDU;
using SMSTerminal.Modem;
using SMSTerminal.SMSMessages;

namespace SMSWindow;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IModemListener, IDisposable, INewSMSListener, IATCommandListener
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private GsmModemConfig _gsmModemConfig = new();
    private int _messagedId;
    private readonly string _testStringLatin = "ABCDEFGHJIKLMNOPQRSTUVWXYZÅÄÖ";
    private readonly string _testStringPunjabi = "ਬਸੰਤ ਸਾਲ ਦਾ ਸਭ ਤੋਂ ਸ਼ਾਨਦਾਰ ਸਮਾਂ ਹੈ।";
    private readonly ModemManager _modemManager = new();
    private bool _formLoaded = false;

    public MainWindow()
    {
        InitializeComponent();

        ModemEventManager.AttachNewSMSListener(this);
        ModemEventManager.AttachModemEventListener(this);
        ModemEventManager.AttachATEventListener(this);

        _gsmModemConfig.BaudRate = BaudRate.Baudrate115200;
        _gsmModemConfig.ComPort = "COM4";
        _gsmModemConfig.ModemTelephoneNumber = "+358400205687";
        _gsmModemConfig.DataBits = 8;
        _gsmModemConfig.Enabled = true;
        _gsmModemConfig.Parity = Parity.None;
        _gsmModemConfig.Stopbits = StopBits.One;
        _gsmModemConfig.LineSignalDtr = true;
        _gsmModemConfig.LineSignalRts = true;
        _gsmModemConfig.Handshake = Handshake.XOnXOff;
        _gsmModemConfig.PIN1 = "0000";
        _gsmModemConfig.ReadTimeout = 10000;
        _gsmModemConfig.WriteTimeout = 10000;
        _gsmModemConfig.DeleteSMSFromModemWhenRead = true;
    }

    public void Dispose()
    {
        ModemEventManager.DetachNewSMSListener(this);
        ModemEventManager.DetachModemEventListener(this);
        ModemEventManager.DetachATEventListener(this);
        _modemManager?.Dispose();
    }

    public void ModemEvent(object sender, ModemEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(e.Message.Trim()) || e.Message.Contains("EmptyData"))
            {
                return;
            }
            var message = $"-------------------------------------\nPort {e.ModemId}\n  Status {e.ResultStatus}\n  EventType {e.EventType}\n  Data :\n ->\n{e.Message}\n<-";
            if (e.ResultStatus.ContainsError())
            {
                if (e.ResultStatus == ModemResultEnum.UnknownModemData)
                {
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxUnknownModemData.Text += Environment.NewLine + message));
                }
                else
                {
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxErrors.Text += Environment.NewLine + message));
                }
            }
            else
            {
                Dispatcher?.BeginInvoke((Action)(() => TextBoxModemLog.Text += Environment.NewLine + message));
            }
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    public void NewSMSEvent(object sender, SMSReceivedEventArgs e)
    {
        var message = "";
        if (e.ShortMessageService.IsStatusReport)
        {
            message = $"-------------------------------------\n[{e.ShortMessageService.SenderTelephone}]" +
                          $"(chars:{e.ShortMessageService.Message.Length})\n->{e.ShortMessageService.Message}<-\n------------------------\n" +
                          $"{e.ShortMessageService.FullPDUInformation}";
        }
        else
        {
            message = $"-------------------------------------\n[{e.ShortMessageService.SenderTelephone}]" +
                          $"(chars:{e.ShortMessageService.Message.Length})\n->{e.ShortMessageService.Message}<-";
        }
        Dispatcher?.BeginInvoke((Action)(() => TextBoxIncomingSMS.Text = TextBoxIncomingSMS.Text + Environment.NewLine + message));
    }
    
    public void ATCommandEvent(object sender, ATCommandEventArgs e)
    {
        if (e.ResultStatus.ContainsError())
        {
            var message = $"-------------------------------------\n{e.ModemId} : {e.Message} {e.ResultStatus} {e.ErrorMessage}";
            Dispatcher?.BeginInvoke((Action)(() => TextBoxErrors.Text = TextBoxErrors.Text + Environment.NewLine + message));
        }
        else
        {
            var message = $"-------------------------------------\n{e.ModemId} : {e.Message} {e.ResultStatus}";
            Dispatcher?.BeginInvoke((Action)(() => TextBoxModemLog.Text = TextBoxModemLog.Text + Environment.NewLine + message));
        }
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        TextBoxTphNumber.Text = Common.RecipientNumber;
        TextBoxSMSText.Text = "ABCDEFGHJIKLMNOPQRSTUVWXYZÅÄÖ";
        SetFormState();
        _formLoaded = true;
    }

    private void SetFormState()
    {
        ButtonConnect.IsEnabled = !_modemManager.HasModems;
        ButtonSettings.IsEnabled = !_modemManager.HasModems;
        ButtonNetworkStatus.IsEnabled = _modemManager.HasModems;
        ButtonSend.IsEnabled = _modemManager.HasModems;
        ButtonReadNewSMS.IsEnabled = _modemManager.HasModems;
        ButtonReadOldSMS.IsEnabled = _modemManager.HasModems;
        ButtonDisconnect.IsEnabled = _modemManager.HasModems;
        ButtonRestartModem.IsEnabled = _modemManager.HasModems;
        ButtonErrorCommand.IsEnabled = _modemManager.HasModems;
        ButtonSetSMSMemory.IsEnabled = _modemManager.HasModems && ComboBoxMemory1Type.SelectedValue != null &&
                                       ComboBoxMemory2Type.SelectedValue != null &&
                                       ComboBoxMemory3Type.SelectedValue != null;
        ButtonReadSMSStats.IsEnabled = _modemManager.HasModems;

        ButtonClearErrors.IsEnabled = !string.IsNullOrEmpty(TextBoxErrors.Text);
        ButtonClearInSMS.IsEnabled = !string.IsNullOrEmpty(TextBoxIncomingSMS.Text);
        ButtonClearModemLog.IsEnabled = !string.IsNullOrEmpty(TextBoxModemLog.Text);
        ButtonClearUnknown.IsEnabled = !string.IsNullOrEmpty(TextBoxUnknownModemData.Text);

        ButtonExecuteATCommand.IsEnabled = _modemManager.HasModems && !string.IsNullOrEmpty(TextBoxATCommand.Text);
        //ButtonClear.IsEnabled = !string.IsNullOrEmpty(TextBox.Text);
    }

    private async void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            if (!await _modemManager.AddTerminal(_gsmModemConfig))
            {
                MessageBox.Show(this, "Failed to add modem.", "Error", MessageBoxButton.OK);
            }
            RefreshModems();
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
        finally
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }

    private void RefreshModems()
    {
        ComboBoxModem.ItemsSource = null;
        ComboBoxModem.SelectedIndex = 0;
        ComboBoxModem.ItemsSource = _modemManager.GetModemList();
    }

    private void GetMemoryInfo()
    {

    }

    private void ButtonDisconnect_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _modemManager.CloseTerminals();
            RefreshModems();
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }

        finally
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }

    private async void ButtonSend_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var outgoingSms = new OutgoingSms
            {
                MessageId = _messagedId++.ToString(),
                Message = TextBoxSMSText.Text,
                SMSEncoding = (SMSEncoding)ComboBoxEncoding.SelectedValue,
                ReceiverTelephone = TextBoxTphNumber.Text,
                RequestStatusReport = CheckBoxStatusReport.IsChecked is true
            };

            TextBoxOutgoingSMSLog.Text = TextBoxOutgoingSMSLog.Text + Environment.NewLine + $"Sending ({outgoingSms.Message.Length}) : " +
                                         Environment.NewLine + outgoingSms.Message;

            if (!await _modemManager.SendSMS(GetSelectedModem(), outgoingSms))
            {
                MessageBox.Show(this, $"Failed to send SMS with Id {outgoingSms.MessageId}.", "Error", MessageBoxButton.OK);
            }
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void UpdateSMSTextBody()
    {
        try
        {
            if (!_formLoaded)
            {
                return;
            }
            string fillerText;
            int endLength;
            string startText;
            string endText;
            Enum.TryParse(ComboBoxEncoding.SelectedItem.ToString(), out SMSEncoding smsEncoding);

            var length = int.Parse(ComboBoxLongSMS.SelectedValue.ToString());
            if (smsEncoding == SMSEncoding._UCS2)
            {
                startText = "ਸ਼ੁਰੂ ਕਰੋ";
                endText = "ਅੰਤ";
                fillerText = _testStringPunjabi;
                length /= 2;
                endLength = 4;
            }
            else
            {
                startText = "START";
                endText = "END";
                fillerText = _testStringLatin;
                endLength = 3;
            }

            TextBoxSMSText.Text = startText;

            while (TextBoxSMSText.Text.Length < length - endLength)
            {
                foreach (var c in fillerText)
                {
                    TextBoxSMSText.Text += c;
                    if (TextBoxSMSText.Text.Length >= length - endLength)
                    {
                        break;
                    }
                }
            }

            TextBoxSMSText.Text += endText;
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Close();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private static void ShowErrorMessageBox(Exception ex, string message = null)
    {
        Logger.Error(ex, message);
        MessageBox.Show(ex.Message, $"Details logged to error log.{Environment.NewLine}{ex.Source}", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async void ButtonNetworkStatus_OnClick(object sender, RoutedEventArgs e)
    {
        await _modemManager.GetNetworkStatus(GetSelectedModem());
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        textBox?.ScrollToEnd();
        SetFormState();
    }

    private void ButtonClearModemLog_OnClick(object sender, RoutedEventArgs e)
    {
        TextBoxModemLog.Clear();
        SetFormState();
    }

    private void ButtonClearIncomingSMS_OnClick(object sender, RoutedEventArgs e)
    {
        TextBoxIncomingSMS.Clear();
        SetFormState();
    }

    private void ButtonClearErrors_OnClick(object sender, RoutedEventArgs e)
    {
        TextBoxErrors.Clear();
        SetFormState();
    }

    private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    {
        try
        {
            Dispose();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void TextBoxSMSText_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        SetFormState();
    }

    private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = new SettingsWindow(_gsmModemConfig);
            if (settingsWindow.ShowDialog() == true)
            {
                _gsmModemConfig = settingsWindow.Settings;
            }
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private async void ButtonReadNewSMS_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Debug("*******************************MANUAL SMS READING*******************************");
            await _modemManager.ReadNewSMS(GetSelectedModem());
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private async void ButtonReadOldSMS_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Debug("*******************************MANUAL SMS READING*******************************");
            await _modemManager.ReadOldSMS(GetSelectedModem());
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void ButtonErrorCommand_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _modemManager.DoError(GetSelectedModem());
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void ComboBoxEncoding_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            UpdateSMSTextBody();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void ComboBoxLongSMS_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            UpdateSMSTextBody();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private async void ButtonRestartModem_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!await _modemManager.RestartModem(GetSelectedModem()))
            {
                MessageBox.Show(this, $"Failed to restart. Either modem not found or error occurred while restarting. {GetSelectedModem()}", "Error", MessageBoxButton.OK);
            }

            RefreshModems();
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void ButtonClearUnknown_OnClick(object sender, RoutedEventArgs e)
    {
        TextBoxUnknownModemData.Text = "";
    }

    private string GetSelectedModem()
    {
        return (string)ComboBoxModem.SelectedValue;
    }

    private async void ButtonExecuteATCommand_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _modemManager.ExecuteATCommand(GetSelectedModem(), TextBoxATCommand.Text, ATTerminationEnum.ATEndPart);
            if (!result.Item1)
            {
                MessageBox.Show(this, $"Failed to execute {TextBoxATCommand.Text}. {GetSelectedModem()}", "Error", MessageBoxButton.OK);
            }
            Dispatcher?.BeginInvoke((Action)(() => TextBoxModemLog.Text += Environment.NewLine + result.Item2.Replace("\r\r","\r")));
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }
    
    private void TextBoxATCommand_OnKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(TextBoxATCommand.Text))
            {
                ButtonExecuteATCommand_OnClick(this, e);
            }
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private async void ButtonSetSMSMemory_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var memoryList = await _modemManager.SetModemMemoryUsed(GetSelectedModem(),
                (ModemMemoryType)ComboBoxMemory1Type.SelectedValue,
                (ModemMemoryType)ComboBoxMemory2Type.SelectedValue,
                (ModemMemoryType)ComboBoxMemory3Type.SelectedValue);

            SetMemoryInformation(memoryList);
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private async void ButtonReadSMSStats_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var memoryList = await _modemManager.ReadModemMemoryStats(GetSelectedModem());
            SetMemoryInformation(memoryList);
            SetFormState();
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void SetMemoryInformation(List<ModemMemory> modemMemoryList)
    {
        if (modemMemoryList == null)
        {
            return;
        }

        LabelMemory1Type.Content = modemMemoryList[0].MemoryType;
        LabelMemory1Used.Content = modemMemoryList[0].MemoryInUse;
        LabelMemory1Total.Content = modemMemoryList[0].MemoryTotal;
        LabelMemory2Type.Content = modemMemoryList[1].MemoryType;
        LabelMemory2Used.Content = modemMemoryList[1].MemoryInUse;
        LabelMemory2Total.Content = modemMemoryList[1].MemoryTotal;
        LabelMemory3Type.Content = modemMemoryList[2].MemoryType;
        LabelMemory3Used.Content = modemMemoryList[2].MemoryInUse;
        LabelMemory3Total.Content = modemMemoryList[2].MemoryTotal;
        ComboBoxMemory1Type.ItemsSource = null;
        ComboBoxMemory1Type.ItemsSource = modemMemoryList[0].MemoryTypesAvailable;
        ComboBoxMemory1Type.SelectedValue = modemMemoryList[0].MemoryType;
        ComboBoxMemory2Type.ItemsSource = null;
        ComboBoxMemory2Type.ItemsSource = modemMemoryList[1].MemoryTypesAvailable;
        ComboBoxMemory2Type.SelectedValue = modemMemoryList[1].MemoryType;
        ComboBoxMemory3Type.ItemsSource = null;
        ComboBoxMemory3Type.ItemsSource = modemMemoryList[2].MemoryTypesAvailable;
        ComboBoxMemory3Type.SelectedValue = modemMemoryList[2].MemoryType;
    }
}