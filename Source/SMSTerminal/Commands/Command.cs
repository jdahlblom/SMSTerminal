﻿using SMSTerminal.Events;
using SMSTerminal.General;

namespace SMSTerminal.Commands
{
    internal class Command
    {
        public Command(string commandString, string terminationString, int numberHolder = 0, string stringHolder = null)
        {
            CommandString = commandString;
            TerminationString = terminationString;
            NumberHolder = numberHolder;
            StringHolder = stringHolder;
        }

        public string CommandString { get; }
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
}
