namespace SMSTerminal.PDU;

/// <summary>
/// PDU Information Element Not Supported
/// </summary>
public class PDUIEINotSupported : PDUInformationElement
{
    internal static PDUIEINotSupported ParseUserDataHeaderHexString(ref string userDataHeaderHex)
    {
        var result = new PDUIEINotSupported();

        if (string.IsNullOrEmpty(userDataHeaderHex))
        {
            throw new ArgumentException("[PDUInformationElementNotSupported] Cannot parse null or empty PDU data.");
        }

        userDataHeaderHex = result.Parse(userDataHeaderHex);
        return result;
    }

    public override string ToString()
    {
        return Environment.NewLine + "PDUInformationElementNotSupported" + Environment.NewLine +
               "-------------------------------------------------" + Environment.NewLine +
               "Showing unprocessed base fields " + Environment.NewLine +
               base.ToString();
    }
}