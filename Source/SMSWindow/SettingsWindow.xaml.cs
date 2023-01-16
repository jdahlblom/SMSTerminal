﻿using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using NLog;
using SMSTerminal.General;
using SMSTerminal.Modem;

namespace SMSWindow;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window, IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly GsmModemConfig _gsmModemConfig = new();

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(GsmModemConfig gsmTerminalConfig)
    {
        InitializeComponent();
        _gsmModemConfig = gsmTerminalConfig;
        ShowSettings();
    }

    public void Dispose()
    {
    }
        
    private void SetFormState()
    {

    }

    private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
    {

    }

    private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
    {

    }
        
    public GsmModemConfig Settings => _gsmModemConfig;

    private void ShowSettings()
    {
        try
        {
            TextBoxComPort.Text = _gsmModemConfig.ComPort;
            ComboBoxBaudRate.SelectedValue = _gsmModemConfig.BaudRate;
            TextBoxDataBits.Text = _gsmModemConfig.DataBits.ToString();
            ComboBoxStopBits.SelectedValue = _gsmModemConfig.Stopbits;
            ComboBoxParity.SelectedValue = _gsmModemConfig.Parity;
            CheckBoxUseDTR.IsChecked = _gsmModemConfig.LineSignalDtr;
            CheckBoxUseRTS.IsChecked = _gsmModemConfig.LineSignalRts;
            ComboBoxHandShake.SelectedValue = _gsmModemConfig.Handshake;
            TextBoxPIN1.Text = _gsmModemConfig.PIN1;
            CheckBoxDeleteReadSMS.IsChecked = _gsmModemConfig.DeleteSMSFromModemWhenRead;
            CheckBoxDisconnectCalls.IsChecked = _gsmModemConfig.AutoDisconnectIncomingCall;
            CheckBoxUseCallForwarding.IsChecked = _gsmModemConfig.UseCallForwarding;
            TextBoxCallForwardTph.Text = _gsmModemConfig.CallForwardingTelephone;
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void SaveSettings()
    {
        try
        {
            _gsmModemConfig.ComPort = TextBoxComPort.Text;
            _gsmModemConfig.BaudRate = (BaudRate)ComboBoxBaudRate.SelectedValue;
            _gsmModemConfig.DataBits = int.Parse(TextBoxDataBits.Text);
            _gsmModemConfig.Stopbits = (StopBits)ComboBoxStopBits.SelectedValue;
            _gsmModemConfig.Parity = (Parity)ComboBoxParity.SelectedValue;
            _gsmModemConfig.LineSignalDtr = CheckBoxUseDTR.IsChecked == true;
            _gsmModemConfig.LineSignalRts = CheckBoxUseRTS.IsChecked == true;
            _gsmModemConfig.Handshake = (Handshake)ComboBoxHandShake.SelectedValue;
            _gsmModemConfig.PIN1 = TextBoxPIN1.Text;
            _gsmModemConfig.DeleteSMSFromModemWhenRead = CheckBoxDeleteReadSMS.IsChecked == true;
            _gsmModemConfig.AutoDisconnectIncomingCall = CheckBoxDisconnectCalls.IsChecked == true;
            _gsmModemConfig.UseCallForwarding = CheckBoxUseCallForwarding.IsChecked == true;
            _gsmModemConfig.CallForwardingTelephone = TextBoxCallForwardTph.Text;
        }
        catch (Exception exception)
        {
            ShowErrorMessageBox(exception);
        }
    }

    private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(TextBoxComPort.Text))
            {
                throw new Exception("Invalid COM Port");
            }

            if (string.IsNullOrEmpty(TextBoxDataBits.Text) || !TextBoxDataBits.Text.IsInt())
            {
                throw new Exception("Invalid Data Bits");
            }
                
            if (CheckBoxUseCallForwarding.IsChecked == true && !TextBoxCallForwardTph.Text.IsValidTph())
            {
                throw new Exception("Invalid Call Forwarding Telephone Number");
            }

            SaveSettings();
            DialogResult = true;
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

    private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}