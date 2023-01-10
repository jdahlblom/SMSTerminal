namespace SMSTerminal.PDU; //Protocol Identifier (TP-PID)



//----------------------------
//Protocol Identifier (TP-PID)
//----------------------------
public enum ProtocolIdentifierType : byte
{
    NORMAL1 = 0x0, // < 0x40
    NORMAL2 = 0x40,
    RESERVED = 0x80,
    SC_SPECIFIC = 0xC0
}

public enum ProtocolIdentifierInterworkingType : byte
{
    SME_TO_SME = 0x0,   //BIT 5  (ProtocolIdentifierMessageTypes)
    TELEMATIC = 0x1     //BIT 5  (ProtocolIdentifierTelematicTypes)
}

public enum ProtocolIdentifierTelematicTypes : byte
{
    SC_SPECIFIC = 0x0,
    TELEX = 0x1,
    GROUP3_TELEFAX = 0x2,
    GROUP4_TELEFAX = 0x3,
    VOICE_TPH = 0x4,
    ERMES = 0x5,
    NATIONAL_PAGING_SYSTEM = 0x6,
    VIDEOTEX = 0x7,
    TELETEX_UNSPECIFIED_CARRIER = 0x8,
    TELETEX_UNSPECIFIED_PSPDN = 0x9,
    TELETEX_UNSPECIFIED_CSPDN = 0xA,
    TELETEX_UNSPECIFIED_PSTN = 0xB,
    TELETEX_UNSPECIFIED_ISDN = 0xC,
    UCI = 0xD,
    RESERVED1 = 0xE,
    RESERVED2 = 0xF,
    KNOWN_MESSAGE_HANDLING_FACILITY = 0x10,
    PUBLIC_X400_BASED = 0x11,
    INTERNET_ELECTRONIC_MAIL = 0x12,
    RESERVED3 = 0x13,
    RESERVED4 = 0x14,
    RESERVED5 = 0x15,
    RESERVED6 = 0x16,
    RESERVED7 = 0x17,
    SC_SPECIFIC1 = 0x18,
    SC_SPECIFIC2 = 0x19,
    SC_SPECIFIC3 = 0x1A,
    SC_SPECIFIC4 = 0x1B,
    SC_SPECIFIC5 = 0x1C,
    SC_SPECIFIC6 = 0x1D,
    SC_SPECIFIC7 = 0x1E,
    GSM_MOBILE_STATION = 0x1F
}

public enum ProtocolIdentifierMessageTypes : byte
{
    SHORT_MESSAGE_TYPE_0 = 0x0,             //ME must acknowledge receipt
    REPLACE_SHORT_MESSAGE_TYPE_1 = 0x1,
    REPLACE_SHORT_MESSAGE_TYPE_2 = 0x2,
    REPLACE_SHORT_MESSAGE_TYPE_3 = 0x3,
    REPLACE_SHORT_MESSAGE_TYPE_4 = 0x4,
    REPLACE_SHORT_MESSAGE_TYPE_5 = 0x5,
    REPLACE_SHORT_MESSAGE_TYPE_6 = 0x6,
    REPLACE_SHORT_MESSAGE_TYPE_7 = 0x7,
    RESERVED1 = 0x8,
    RESERVED2 = 0x9,
    RESERVED3 = 0xA,
    RESERVED4 = 0xB,
    RESERVED5 = 0xC,
    RESERVED6 = 0xD,
    RESERVED7 = 0xE,
    RESERVED8 = 0xF,
    RESERVED9 = 0x10,
    RESERVED10 = 0x11,
    RESERVED11 = 0x12,
    RESERVED12 = 0x13,
    RESERVED13 = 0x14,
    RESERVED14 = 0x15,
    RESERVED15 = 0x16,
    RESERVED16 = 0x17,
    RESERVED17 = 0x18,
    RESERVED18 = 0x19,
    RESERVED19 = 0x1A,
    RESERVED20 = 0x1B,
    RESERVED21 = 0x1C,
    RESERVED22 = 0x1D,
    RESERVED23 = 0x1E,
    RETURN_CALL_MESSAGE = 0x1F,
    RESERVED24 = 0x20,
    RESERVED25 = 0x21,
    RESERVED26 = 0x22,
    RESERVED27 = 0x23,
    RESERVED28 = 0x24,
    RESERVED29 = 0x25,
    RESERVED30 = 0x26,
    RESERVED31 = 0x27,
    RESERVED32 = 0x28,
    RESERVED33 = 0x29,
    RESERVED34 = 0x2A,
    RESERVED35 = 0x2B,
    RESERVED36 = 0x2C,
    RESERVED37 = 0x2D,
    RESERVED38 = 0x2E,
    RESERVED39 = 0x2F,
    RESERVED40 = 0x30,
    RESERVED41 = 0x31,
    RESERVED42 = 0x32,
    RESERVED43 = 0x33,
    RESERVED44 = 0x34,
    RESERVED45 = 0x35,
    RESERVED46 = 0x36,
    RESERVED47 = 0x37,
    RESERVED48 = 0x38,
    RESERVED49 = 0x39,
    RESERVED50 = 0x3A,
    RESERVED51 = 0x3B,
    RESERVED52 = 0x3C,
    ME_DATA_DOWNLOAD = 0x3D,
    ME_DE_PERSONALIZATION_SME = 0x3E,
    SIM_DATA_DOWNLOAD = 0x3F
}

//----------------------------
//END Protocol Identifier (TP-PID)
//----------------------------

public class PDUProtocolIdentifier
{

    private ProtocolIdentifierType _protocolIdentifierType;
    private ProtocolIdentifierInterworkingType _protocolIdentifierInterworkingType;
    private ProtocolIdentifierTelematicTypes _protocolIdentifierTelematicTypes;
    private ProtocolIdentifierMessageTypes _protocolIdentifierMessageTypes;

    public PDUProtocolIdentifier()
    {
    }

    public PDUProtocolIdentifier(byte octet)
    {
        ParseOctet(octet);
    }

    private void ParseOctet(byte octet)
    {
        var type = octet & 0xC0;
        if(type < 0x40)
        {
            _protocolIdentifierType = ProtocolIdentifierType.NORMAL1;
        }else
        {
            _protocolIdentifierType = (ProtocolIdentifierType) type;
        }

        if(_protocolIdentifierType == ProtocolIdentifierType.NORMAL1)
        {
            _protocolIdentifierInterworkingType = (ProtocolIdentifierInterworkingType) (byte)(octet & 0x20);
        }
            
        if(_protocolIdentifierInterworkingType == ProtocolIdentifierInterworkingType.TELEMATIC)
        {
            _protocolIdentifierTelematicTypes = (ProtocolIdentifierTelematicTypes)(byte)(octet & 0x1F);
        }
        if (_protocolIdentifierInterworkingType == ProtocolIdentifierInterworkingType.SME_TO_SME)
        {
            _protocolIdentifierMessageTypes = (ProtocolIdentifierMessageTypes)(byte)(octet & 0x3F);
        }
    }

    public byte GetOctet()
    {
        byte result = 0;
        result = (byte)(result | (byte)_protocolIdentifierType);
        if(_protocolIdentifierType == ProtocolIdentifierType.NORMAL1)
        {
            result = (byte)(result | (byte)_protocolIdentifierInterworkingType);
        }
        if (_protocolIdentifierInterworkingType == ProtocolIdentifierInterworkingType.TELEMATIC)
        {
            result = (byte)(result | (byte)_protocolIdentifierTelematicTypes);
        }
        if (_protocolIdentifierInterworkingType == ProtocolIdentifierInterworkingType.SME_TO_SME)
        {
            result = (byte)(result | (byte)_protocolIdentifierMessageTypes);
        }
        return result;
    }

    public ProtocolIdentifierType ProtocolIdentifierType
    {
        get => _protocolIdentifierType;
        set => _protocolIdentifierType = value;
    }

    public ProtocolIdentifierInterworkingType ProtocolIdentifierInterworkingType
    {
        get => _protocolIdentifierInterworkingType;
        set => _protocolIdentifierInterworkingType = value;
    }

    public ProtocolIdentifierTelematicTypes ProtocolIdentifierTelematicTypes
    {
        get => _protocolIdentifierTelematicTypes;
        set => _protocolIdentifierTelematicTypes = value;
    }

    public ProtocolIdentifierMessageTypes ProtocolIdentifierMessageTypes
    {
        get => _protocolIdentifierMessageTypes;
        set => _protocolIdentifierMessageTypes = value;
    }

    public override string ToString()
    {
        return string.Format("ProtocolIdentifierType: {0}" + Environment.NewLine +
                             "ProtocolIdentifierInterworkingType: {1}" + Environment.NewLine +
                             "ProtocolIdentifierTelematicTypes: {2}" + Environment.NewLine + 
                             "ProtocolIdentifierMessageTypes: {3}", _protocolIdentifierType, _protocolIdentifierInterworkingType, _protocolIdentifierTelematicTypes, _protocolIdentifierMessageTypes);
    }
}