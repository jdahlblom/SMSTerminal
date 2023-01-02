namespace SMSTerminal.General
{


    public enum BaudRate
    {
        Baudrate9600 = 9600,
        Baudrate19200 = 19200,
        Baudrate38400 = 38400,
        Baudrate57600 = 57600,
        Baudrate115200 = 115200,
        Baudrate230400 = 230400
    }

    public enum EncodingType
    {
        Ascii,
        Unicode,
        Utf7,
        Utf8
    }

    public enum CallForwardingStatus
    {
        NoneActive = 0,
        AllActive = 1,
        SomeActive = 2,
        Unknown = 4
    }

    public enum ModemType
    {
        PDU //Other modem types used removed as this code works with all tested modems
    }

    public enum ErrorType
    {
        CMS = 100,
        CME = 200
    }

    public enum SmsDirection : byte
    {
        Outgoing = 1,
        Incoming = 2,
        Both = 3
    };
    
    public enum GsmNetworkRegistrationStatus : byte
    {
        NotRegisteredNotSearching = 0,
        RegisteredToHomeNetwork = 1,
        NotRegisteredSearching = 2,
        RegistrationDenied = 3,
        Unknown = 4,
        RegisteredRoaming = 5
    };

    public enum PINAuthenticationStatus : byte
    {
        StatusOK = 0,                                       //READY		    SIM ready to be used
        AwaitingPIN1 = 1,                                   //SIM PIN	    ME is waiting for SIM PIN1.
        AwaitingPUK1 = 2,                                   //SIM PUK       ME is waiting for SIM PUK1 if PIN1 was disabled after three failed attempts to enter PIN1.
        AwaitingPIN2 = 3,                                   //SIM PIN2		ME is waiting for PIN2. This is only applicable when an attempt to access a
                                                            //              PIN2 related feature was acknowledged with +CME ERROR: 17 ("SIM PIN2
                                                            //              required"), for example when the client attempts to edit the FD phonebook). In
                                                            //              this case the read command AT+CPIN? also prompts for SIM PIN2. Normally,
                                                            //              the AT+CPIN2 command is intended for SIM PIN2.
        AwaitingPUK2 = 4,                                   //SIM PUK2		ME is waiting for PUK2 to unblock a disabled PIN2. As above, this is only necessary
                                                            //              when the preceding command was acknowledged with +CME ERROR:
                                                            //              18 ("SIM PUK2 required") and only if the read command AT+CPIN? also
                                                            //              prompts for SIM PUK2. Normally, the AT+CPIN2 command is intended for SIM
                                                            //              PUK2.
                                                            //              Phone security locks set by client or factory
        AwaitingPhoneToSIMPIN = 5,                          //PH-SIM PIN	ME is waiting for phone-to-SIM card password if "PS" lock is active and the client
                                                            //              inserts other SIM card than the one used for the lock. ("PS" lock is also
                                                            //              referred to as phone or antitheft lock).
        AwaitingMasterPhoneCodePUK = 6,                     //PH-SIM PUK	ME is waiting for Master Phone Code, if the above "PS" lock password was
                                                            //              incorrectly entered three times.
        AwaitingPhoneToVerifyFirstSIMPIN = 7,               //PH-FSIM PIN	ME is waiting for phone-to-very-first-SIM card. Necessary when "PF" lock was
                                                            //              set. When powered up the first time, ME locks itself to the first SIM card put into
                                                            //              the card holder. As a result, operation of the mobile is restricted to this one SIM
                                                            //              card (unless the PH-FSIM PUK is used as described below).
        AwaitingPhoneToVerifyFirstSIMPUK = 8,               //PH-FSIM PUK	ME is waiting for phone-to-very-first-SIM card unblocking password to be
                                                            //              given. Necessary when "PF" lock is active and other than first SIM card is inserted.
        AwaitingPhoneToNetworkPersonalisationPUK = 9,       //PH-NET PUK	ME is waiting for network personalisation unblocking password
        AwaitingNetworkSubsetPersonalisationPIN = 10,       //PH-NS PIN		ME is waiting for network subset personalisation password
        AwaitingNetworkSubsetPersonalisationPUK = 11,       //PH-NS PUK		ME is waiting for network subset unblocking password
        AwaitingServiceProviderPersonalisationPIN = 12,     //PH-SP PIN		ME is waiting for service provider personalisation password
        AwaitingServiceProviderPersonalisationPUK = 13,     //PH-SP PUK		ME is waiting for service provider personalisation unblocking password
        AwaitingCorporatePersonalisationPIN = 14,           //PH-C PIN		ME is waiting for corporate personalisation password
        AwaitingCorporatePersonalisationPUK = 15,           //PH-C PUK		ME is waiting for corporate personalisation un-blocking password
        SIMBlocked = 16,                                    //
        SIMBusy = 17,                                       //
        AuthenticationStatusUnknown = 99                    //FAILED TO PARSE REPLY FROM MODEM.
    };


    /// <summary>
    /// Information Element Identifier Enum
    /// NS_  = "Not Supported" by SMSTerminal
    /// </summary>
    public enum IEIEnum : byte
    {
        Concatenated_Short_Messages_8Bit_Reference = 0x0,
        NS_Special_SMS_Message_Indication = 0x1,
        NS_Reserved = 0x2,
        NS_Not_Used = 0x3,
        NS_Application_Port_Addressing_Scheme_8Bit = 0x4,
        NS_Application_Port_Addressing_Scheme_16Bit = 0x5,
        NS_SMSC_Control_Parameters = 0x6,
        NS_UDH_Source_Indicator = 0x7,
        Concatenated_Short_Messages_16Bit_Reference = 0x8,
        NS_Wireless_Control_Message_Protocol = 0x9,
        NS_Reserved_0x29 = 0x29,
        NS_Reserved_0x2A = 0x2A,
        NS_Reserved_0x2B = 0x2B,
        NS_Reserved_0x2C = 0x2C,
        NS_Reserved_0x2D = 0x2D,
        NS_Reserved_0x2E = 0x2E,
        NS_Reserved_0x2F = 0x2F,
        NS_Reserved_0x30 = 0x30,
        NS_Reserved_0x31 = 0x31,
        NS_Reserved_0x32 = 0x32,
        NS_Reserved_0x33 = 0x33,
        NS_Reserved_0x34 = 0x34,
        NS_Reserved_0x35 = 0x35,
        NS_Reserved_0x36 = 0x36,
        NS_Reserved_0x37 = 0x37,
        NS_Reserved_0x38 = 0x38,
        NS_Reserved_0x39 = 0x39,
        NS_Reserved_0x3A = 0x3A,
        NS_Reserved_0x3B = 0x3B,
        NS_Reserved_0x3C = 0x3C,
        NS_Reserved_0x3D = 0x3D,
        NS_Reserved_0x3E = 0x3E,
        NS_Reserved_0x3F = 0x3F,
        NS_Reserved_0x40 = 0x40,
        NS_Reserved_0x41 = 0x41,
        NS_Reserved_0x42 = 0x42,
        NS_Reserved_0x43 = 0x43,
        NS_Reserved_0x44 = 0x44,
        NS_Reserved_0x45 = 0x45,
        NS_Reserved_0x46 = 0x46,
        NS_Reserved_0x47 = 0x47,
        NS_Reserved_0x48 = 0x48,
        NS_Reserved_0x49 = 0x49,
        NS_Reserved_0x4A = 0x4A,
        NS_Reserved_0x4B = 0x4B,
        NS_Reserved_0x4C = 0x4C,
        NS_Reserved_0x4D = 0x4D,
        NS_Reserved_0x4E = 0x4E,
        NS_Reserved_0x4F = 0x4F,
        NS_Reserved_0x50 = 0x50,
        NS_Reserved_0x51 = 0x51,
        NS_Reserved_0x52 = 0x52,
        NS_Reserved_0x53 = 0x53,
        NS_Reserved_0x54 = 0x54,
        NS_Reserved_0x55 = 0x55,
        NS_Reserved_0x56 = 0x56,
        NS_Reserved_0x57 = 0x57,
        NS_Reserved_0x58 = 0x58,
        NS_Reserved_0x59 = 0x59,
        NS_Reserved_0x5A = 0x5A,
        NS_Reserved_0x5B = 0x5B,
        NS_Reserved_0x5C = 0x5C,
        NS_Reserved_0x5D = 0x5D,
        NS_Reserved_0x5E = 0x5E,
        NS_Reserved_0x5F = 0x5F,
        NS_Reserved_0x60 = 0x60,
        NS_Reserved_0x61 = 0x61,
        NS_Reserved_0x62 = 0x62,
        NS_Reserved_0x63 = 0x63,
        NS_Reserved_0x64 = 0x64,
        NS_Reserved_0x65 = 0x65,
        NS_Reserved_0x66 = 0x66,
        NS_Reserved_0x67 = 0x67,
        NS_Reserved_0x68 = 0x68,
        NS_Reserved_0x69 = 0x69,
        NS_Reserved_0x6A = 0x6A,
        NS_Reserved_0x6B = 0x6B,
        NS_Reserved_0x6C = 0x6C,
        NS_Reserved_0x6D = 0x6D,
        NS_Reserved_0x6E = 0x6E,
        NS_Reserved_0x6F = 0x6F,
        NS_SIM_Toolkit_Security_Headers_0x70 = 0x70,
        NS_SIM_Toolkit_Security_Headers_0x71 = 0x71,
        NS_SIM_Toolkit_Security_Headers_0x72 = 0x72,
        NS_SIM_Toolkit_Security_Headers_0x73 = 0x73,
        NS_SIM_Toolkit_Security_Headers_0x74 = 0x74,
        NS_SIM_Toolkit_Security_Headers_0x75 = 0x75,
        NS_SIM_Toolkit_Security_Headers_0x76 = 0x76,
        NS_SIM_Toolkit_Security_Headers_0x77 = 0x77,
        NS_SIM_Toolkit_Security_Headers_0x78 = 0x78,
        NS_SIM_Toolkit_Security_Headers_0x79 = 0x79,
        NS_SIM_Toolkit_Security_Headers_0x7A = 0x7A,
        NS_SIM_Toolkit_Security_Headers_0x7B = 0x7B,
        NS_SIM_Toolkit_Security_Headers_0x7C = 0x7C,
        NS_SIM_Toolkit_Security_Headers_0x7D = 0x7D,
        NS_SIM_Toolkit_Security_Headers_0x7E = 0x7E,
        NS_SIM_Toolkit_Security_Headers_0x7F = 0x7F,
        NS_SME_to_SME_Specific_0x80 = 0x80,
        NS_SME_to_SME_Specific_0x81 = 0x81,
        NS_SME_to_SME_Specific_0x82 = 0x82,
        NS_SME_to_SME_Specific_0x83 = 0x83,
        NS_SME_to_SME_Specific_0x84 = 0x84,
        NS_SME_to_SME_Specific_0x85 = 0x85,
        NS_SME_to_SME_Specific_0x86 = 0x86,
        NS_SME_to_SME_Specific_0x87 = 0x87,
        NS_SME_to_SME_Specific_0x88 = 0x88,
        NS_SME_to_SME_Specific_0x89 = 0x89,
        NS_SME_to_SME_Specific_0x8A = 0x8A,
        NS_SME_to_SME_Specific_0x8B = 0x8B,
        NS_SME_to_SME_Specific_0x8C = 0x8C,
        NS_SME_to_SME_Specific_0x8D = 0x8D,
        NS_SME_to_SME_Specific_0x8E = 0x8E,
        NS_SME_to_SME_Specific_0x8F = 0x8F,
        NS_SME_to_SME_Specific_0x90 = 0x90,
        NS_SME_to_SME_Specific_0x91 = 0x91,
        NS_SME_to_SME_Specific_0x92 = 0x92,
        NS_SME_to_SME_Specific_0x93 = 0x93,
        NS_SME_to_SME_Specific_0x94 = 0x94,
        NS_SME_to_SME_Specific_0x95 = 0x95,
        NS_SME_to_SME_Specific_0x96 = 0x96,
        NS_SME_to_SME_Specific_0x97 = 0x97,
        NS_SME_to_SME_Specific_0x98 = 0x98,
        NS_SME_to_SME_Specific_0x99 = 0x99,
        NS_SME_to_SME_Specific_0x9A = 0x9A,
        NS_SME_to_SME_Specific_0x9B = 0x9B,
        NS_SME_to_SME_Specific_0x9C = 0x9C,
        NS_SME_to_SME_Specific_0x9D = 0x9D,
        NS_SME_to_SME_Specific_0x9E = 0x9E,
        NS_SME_to_SME_Specific_0x9F = 0x9F,
        NS_Reserved_0xA0 = 0xA0,
        NS_Reserved_0xA1 = 0xA1,
        NS_Reserved_0xA2 = 0xA2,
        NS_Reserved_0xA3 = 0xA3,
        NS_Reserved_0xA4 = 0xA4,
        NS_Reserved_0xA5 = 0xA5,
        NS_Reserved_0xA6 = 0xA6,
        NS_Reserved_0xA7 = 0xA7,
        NS_Reserved_0xA8 = 0xA8,
        NS_Reserved_0xA9 = 0xA9,
        NS_Reserved_0xAA = 0xAA,
        NS_Reserved_0xAB = 0xAB,
        NS_Reserved_0xAC = 0xAC,
        NS_Reserved_0xAD = 0xAD,
        NS_Reserved_0xAE = 0xAE,
        NS_Reserved_0xAF = 0xAF,
        NS_Reserved_0xB0 = 0xB0,
        NS_Reserved_0xB1 = 0xB1,
        NS_Reserved_0xB2 = 0xB2,
        NS_Reserved_0xB3 = 0xB3,
        NS_Reserved_0xB4 = 0xB4,
        NS_Reserved_0xB5 = 0xB5,
        NS_Reserved_0xB6 = 0xB6,
        NS_Reserved_0xB7 = 0xB7,
        NS_Reserved_0xB8 = 0xB8,
        NS_Reserved_0xB9 = 0xB9,
        NS_Reserved_0xBA = 0xBA,
        NS_Reserved_0xBB = 0xBB,
        NS_Reserved_0xBC = 0xBC,
        NS_Reserved_0xBD = 0xBD,
        NS_Reserved_0xBE = 0xBE,
        NS_Reserved_0xBF = 0xBF,
        NS_SC_Specific_0xC0 = 0xC0,
        NS_SC_Specific_0xC1 = 0xC1,
        NS_SC_Specific_0xC2 = 0xC2,
        NS_SC_Specific_0xC3 = 0xC3,
        NS_SC_Specific_0xC4 = 0xC4,
        NS_SC_Specific_0xC5 = 0xC5,
        NS_SC_Specific_0xC6 = 0xC6,
        NS_SC_Specific_0xC7 = 0xC7,
        NS_SC_Specific_0xC8 = 0xC8,
        NS_SC_Specific_0xC9 = 0xC9,
        NS_SC_Specific_0xCA = 0xCA,
        NS_SC_Specific_0xCB = 0xCB,
        NS_SC_Specific_0xCC = 0xCC,
        NS_SC_Specific_0xCD = 0xCD,
        NS_SC_Specific_0xCE = 0xCE,
        NS_SC_Specific_0xCF = 0xCF,
        NS_SC_Specific_0xD0 = 0xD0,
        NS_SC_Specific_0xD1 = 0xD1,
        NS_SC_Specific_0xD2 = 0xD2,
        NS_SC_Specific_0xD3 = 0xD3,
        NS_SC_Specific_0xD4 = 0xD4,
        NS_SC_Specific_0xD5 = 0xD5,
        NS_SC_Specific_0xD6 = 0xD6,
        NS_SC_Specific_0xD7 = 0xD7,
        NS_SC_Specific_0xD8 = 0xD8,
        NS_SC_Specific_0xD9 = 0xD9,
        NS_SC_Specific_0xDA = 0xDA,
        NS_SC_Specific_0xDB = 0xDB,
        NS_SC_Specific_0xDC = 0xDC,
        NS_SC_Specific_0xDD = 0xDD,
        NS_SC_Specific_0xDE = 0xDE,
        NS_SC_Specific_0xDF = 0xDF,
        NS_Reserved_0xE0 = 0xE0,
        NS_Reserved_0xE1 = 0xE1,
        NS_Reserved_0xE2 = 0xE2,
        NS_Reserved_0xE3 = 0xE3,
        NS_Reserved_0xE4 = 0xE4,
        NS_Reserved_0xE5 = 0xE5,
        NS_Reserved_0xE6 = 0xE6,
        NS_Reserved_0xE7 = 0xE7,
        NS_Reserved_0xE8 = 0xE8,
        NS_Reserved_0xE9 = 0xE9,
        NS_Reserved_0xEA = 0xEA,
        NS_Reserved_0xEB = 0xEB,
        NS_Reserved_0xEC = 0xEC,
        NS_Reserved_0xED = 0xED,
        NS_Reserved_0xEE = 0xEE,
        NS_Reserved_0xEF = 0xEF,
        NS_Reserved_0xF0 = 0xF0,
        NS_Reserved_0xF1 = 0xF1,
        NS_Reserved_0xF2 = 0xF2,
        NS_Reserved_0xF3 = 0xF3,
        NS_Reserved_0xF4 = 0xF4,
        NS_Reserved_0xF5 = 0xF5,
        NS_Reserved_0xF6 = 0xF6,
        NS_Reserved_0xF7 = 0xF7,
        NS_Reserved_0xF8 = 0xF8,
        NS_Reserved_0xF9 = 0xF9,
        NS_Reserved_0xFA = 0xFA,
        NS_Reserved_0xFB = 0xFB,
        NS_Reserved_0xFC = 0xFC,
        NS_Reserved_0xFD = 0xFD,
        NS_Reserved_0xFE = 0xFE,
        NS_Reserved_0xFF = 0xFF
    }
}
