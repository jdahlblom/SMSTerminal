using SMSTerminal.General;

namespace SMSTerminal.PDU;

public class PDUInformationElement
{
    /// <summary>
    /// Information Element Identifier
    /// </summary>
    public IEIEnum IEI { get; set; }

    protected byte InformationElementLength { get; private set; }

    protected string InformationElementDataHex { get; private set; }

    protected string Parse(string userDataHeaderHex)
    {
        var tempUserDataHeaderHex = userDataHeaderHex;
        IEI = (IEIEnum)Convert.ToByte(tempUserDataHeaderHex[..2], 16);
        tempUserDataHeaderHex = tempUserDataHeaderHex[2..];

        InformationElementLength = Convert.ToByte(tempUserDataHeaderHex[..2], 16);
        tempUserDataHeaderHex = tempUserDataHeaderHex[2..];

        InformationElementDataHex = tempUserDataHeaderHex[..(InformationElementLength * 2)];

        return tempUserDataHeaderHex[(InformationElementLength * 2)..];
    }

    public virtual byte[] GetBytes()
    {
        if (!string.IsNullOrEmpty(InformationElementDataHex))
        {
            return Functions.HexStringToByteArray(InformationElementDataHex);
        }

        return null;
    }

    public override string ToString()
    {
        return $"InformationElementData: {InformationElementDataHex}, InformationElementIdentifier: {IEI}, InformationElementLength: {InformationElementLength}";
    }
}