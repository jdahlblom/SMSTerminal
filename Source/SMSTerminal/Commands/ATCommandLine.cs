using SMSTerminal.Events;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;

/// <summary>
/// Holds the actual AT command and termination character.
/// </summary>
internal class ATCommandLine
{
    public ATCommandLine(string atCommandString, string terminationString, int numberHolder = 0, string stringHolder = null)
    {
        ATCommandString = atCommandString;
        TerminationString = terminationString;
        NumberHolder = numberHolder;
        StringHolder = stringHolder;
    }

    public ATCommandLine(string atCommandInformation, string atCommandString, string terminationString, int numberHolder = 0, string stringHolder = null)
    {
        ATCommandInformation = atCommandInformation;
        ATCommandString = atCommandString;
        TerminationString = terminationString;
        NumberHolder = numberHolder;
        StringHolder = stringHolder;
    }

    public string ATCommandInformation { get; }
    public string ATCommandString { get; }
    public string TerminationString { get; }
    private bool HasResult => ModemData != null && ModemData.ModemResult != ModemResultEnum.None;
    public ModemData ModemData { get; set; }

    /// <summary>
    /// These two fields can be used to hold special information
    /// so that when the modem result are processed it is easy to
    /// recall what was done. For example deleting SMS. Setting
    /// NumberHolder to the memory slot being deleted makes it easy
    /// to retrieve this when the modem returns without parsing the
    /// AT command.
    /// </summary>
    public string StringHolder { get; set; }
    public int NumberHolder { get; set; }
}