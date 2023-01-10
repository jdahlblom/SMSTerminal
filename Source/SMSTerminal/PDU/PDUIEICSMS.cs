using SMSTerminal.General;

namespace SMSTerminal.PDU;

/// <summary>
/// PDU Information Element Identifier CSMS
/// </summary>
public class PDUIEICSMS : PDUInformationElement
{
    //-----------------------------------------------------------------------------------------------
    //Information Element Identifier *Header*
    //-----------------------------------------------------------------------------------------------
    // 1 octet
    //private InformationElementIdentifier _headerField1InformationElementIdentifier = InformationElementIdentifier.Concatenated_Short_Messages_16Bit_Reference;

    // 1 octet
    //private byte _headerField2InformationElementDataLength = 0x04; //0x03 -> 8-bit reference, 0x04 -> 16-bit reference
    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------


    //-----------------------------------------------------------------------------------------------
    //  Information Element Identifier *Data*
    //-----------------------------------------------------------------------------------------------
    /* Field 3, 1 or 2 Octet(s) 00-FF or 0000-FFFF */
    private ushort _dataField1CSMSMessageReference;//00-FF or 0000-FFFF, must be the same for all parts, reference for the CSMS[Concatenated SMS Reference] not equal to TP-Message-Reference

    /* Field 4, 1 Octet */
    private byte _dataField2PartsTotal;//00-FF, total number of parts

    /* Field 5, 1 Octet*/
    private byte _dataField3ThisPart;//00-FF
    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------

    /*public static bool IsOfType(string pdu)
    {
        if(string.IsNullOrEmpty(pdu) || pdu.Length < 2)
        {
            return false;
        }
        var informationElementIdentifier = (InformationElementIdentifier) Convert.ToByte(pdu.Substring(0, 2), 16);
        return informationElementIdentifier == InformationElementIdentifier.Concatenated_Short_Messages_8Bit_Reference || informationElementIdentifier == InformationElementIdentifier.Concatenated_Short_Messages_16Bit_Reference;
    }*/
    /*
    public override InformationElementIdentifierEnum IEIType()
    {
        return InformationElementIdentifier;
    }
    */
    internal static PDUIEICSMS ParseUserDataHeaderHexString(ref string userDataHeaderHex)
    {
        var result = new PDUIEICSMS();

        if (string.IsNullOrEmpty(userDataHeaderHex))
        {
            throw new ArgumentException("[PDUInformationElementConcatenatedMessage] Cannot parse null or empty PDU data.");
        }

        // Headers are here
        userDataHeaderHex = result.Parse(userDataHeaderHex);

        /* 
         * --------------------
         * DATA BELOW             
         * --------------------
         */
        var dataAsHex = result.InformationElementDataHex;

        if (result.IEI == IEIEnum.Concatenated_Short_Messages_8Bit_Reference)
        {
            result.MessageReference = Convert.ToUInt16(dataAsHex[..2], 16);
            dataAsHex = dataAsHex[2..];
        }
        else if (result.IEI == IEIEnum.Concatenated_Short_Messages_16Bit_Reference)
        {
            result._dataField1CSMSMessageReference = Convert.ToUInt16(dataAsHex[..4], 16);
            dataAsHex = dataAsHex[4..];
        }

        result._dataField2PartsTotal = Convert.ToByte(dataAsHex[..2], 16);
        dataAsHex = dataAsHex[2..];

        result._dataField3ThisPart = Convert.ToByte(dataAsHex[..2], 16);

        /* 
         * --------------------
         * DATA END
         * --------------------
         */

        return result;

    }

    public override byte[] GetBytes()
    {
        var bytes = new List<byte>
        {
            (byte)IEI,
            IEI == IEIEnum.Concatenated_Short_Messages_8Bit_Reference ? (byte)2 : (byte)4
        };
        if (IEI == IEIEnum.Concatenated_Short_Messages_8Bit_Reference)
        {
            var b = Convert.ToByte(_dataField1CSMSMessageReference & 0x00FF);
            bytes.Add(b);
        }
        else
        {
            var b = Convert.ToByte((_dataField1CSMSMessageReference & 0xFF00) >> 8);
            bytes.Add(b);
            b = Convert.ToByte(_dataField1CSMSMessageReference & 0x00FF);
            bytes.Add(b);
        }
        bytes.Add(_dataField2PartsTotal);
        bytes.Add(_dataField3ThisPart);
        return bytes.ToArray();
    }

    public static int GenerateCSMSMessageReference()
    {
        return Functions.GetRandom(ushort.MinValue, ushort.MaxValue);
    }

    public int MessageReference
    {
        get => _dataField1CSMSMessageReference;
        set
        {
            if (value > ushort.MaxValue)
            {
                throw new ArgumentException($"MessageReference, value must be <= {ushort.MaxValue}");
            }
            _dataField1CSMSMessageReference = (ushort)value;
        }
    }

    public int MessagePartsTotal
    {
        get => _dataField2PartsTotal;
        set
        {
            if (value > byte.MaxValue)
            {
                throw new ArgumentException("MessagePartsTotal, value must be <= 255");
            }
            _dataField2PartsTotal = (byte)value;
        }
    }

    public int ThisPart
    {
        get => _dataField3ThisPart;
        set
        {
            if (value > byte.MaxValue)
            {
                throw new ArgumentException("ThisPart, value must be <= 255");
            }
            _dataField3ThisPart = (byte)value;
        }
    }

    /*public InformationElementIdentifier InformationElementIdentifier
    {
        get { return InformationElementIdentifier; }
        set { InformationElementIdentifier = value; }
    }

    public byte HeaderField2InformationElementDataLength
    {
        get { return _headerField2InformationElementDataLength; }
        set { _headerField2InformationElementDataLength = value; }
    }*/

    public override string ToString()
    {
        return string.Format(Environment.NewLine + "PDUInformationElementConcatenatedMessage" + Environment.NewLine +
                             "-------------------------------------------------" + Environment.NewLine +
                             "{0}" + Environment.NewLine +
                             "DataField1CSMSMessageReference: {1}" + Environment.NewLine +
                             "DataField2PartsTotal: {2}" + Environment.NewLine +
                             "DataField3ThisPart: {3}" + Environment.NewLine +
                             "HeaderField1InformationElementIdentifier: {4}" + Environment.NewLine +
                             "HeaderField2InformationElementDataLength: {5}" + Environment.NewLine +
                             "-------------------------------------------------" + Environment.NewLine, 
            "", _dataField1CSMSMessageReference, _dataField2PartsTotal, _dataField3ThisPart, IEI, InformationElementLength);
    }//"" was base.ToString()
}