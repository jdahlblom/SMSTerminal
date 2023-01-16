namespace SMSTerminal.Modem;

/// <summary>
/// Logical memory type, depending of how it is used.
/// </summary>
public enum ModemStorageType
{
    /// <summary>
    /// Memory to be used when listing, reading and deleting messages
    /// </summary>
    Memory1,
    /// <summary>
    /// Memory to be used when writing and sending messages
    /// </summary>
    Memory2,
    /// <summary>
    /// Received messages memory storage
    /// </summary>
    Memory3
}

/// <summary>
/// Type of memory, SIM or ME or other
/// </summary>
public enum ModemMemoryType
{
    /// <summary>
    /// SIM memory
    /// </summary>
    SM,
    /// <summary>
    /// Mobile terminal memory
    /// </summary>
    ME,
    /// <summary>
    /// Includes the all of the above memory
    /// </summary>
    MT
}

/// <summary>
/// Memory used by modem.
/// </summary>
public class ModemMemory
{
    public string ModemId { get; set; }
    public ModemStorageType StorageType { get; set; }
    public ModemMemoryType MemoryType { get; set; }
    public List<ModemMemoryType> MemoryTypesAvailable { get; set; } = new();
    public int MemoryInUse { get; set; }
    public int MemoryTotal { get; set; }
}