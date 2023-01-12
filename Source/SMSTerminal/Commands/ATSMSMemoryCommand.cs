using SMSTerminal.General;
using SMSTerminal.Interfaces;
using SMSTerminal.Modem;

namespace SMSTerminal.Commands;


/// <summary>
/// Gets available memory types on ME.
/// Sets preferred memory type to be used to store SMS.
/// Gets available free memory on ME.
///
/// +CPMS: ("ME","SM","MT"),("ME","SM","MT"),("SM","MT")
/// ME = Mobile Equipment internal memory
/// SM = SIM card
/// MT = sum of "SM" and "ME" storage
/// Varies depending on modem.
/// </summary>
internal class ATSMSMemoryCommand : ATCommandBase
{
    private readonly MemoryCommandMode _memoryCommandMode;
    public readonly ModemMemory Memory1 = new() { StorageType = ModemStorageType.Memory1 };
    public readonly ModemMemory Memory2 = new() { StorageType = ModemStorageType.Memory2 };
    public readonly ModemMemory Memory3 = new() { StorageType = ModemStorageType.Memory3 };
    public readonly List<ModemMemory> ModemMemoryList = new();
    private ModemMemory _memoryReadTo;

    private enum MemoryCommandMode
    {
        GetMemoryInformation,
        SetMemoryTypesUsed
    }


    public ATSMSMemoryCommand(IModem modem)
    {
        Modem = modem;
        SetLists();
        _memoryCommandMode = MemoryCommandMode.GetMemoryInformation;
        CommandType = "[AT SMS Memory Command]";
        ATCommandsList.Add(new ATCommand(ATCommands.ATGetAvailableMemoryTypes, ATCommands.ATEndPart));
        ATCommandsList.Add(new ATCommand(ATCommands.ATGetChosenMemoryUsage, ATCommands.ATEndPart));
    }

    public ATSMSMemoryCommand(IModem modem, ModemMemoryType memory1TypeToUse, ModemMemoryType memory2TypeToUse, ModemMemoryType memory3TypeToUse)
    {
        Modem = modem;
        SetLists();
        _memoryCommandMode = MemoryCommandMode.SetMemoryTypesUsed;
        CommandType = "[AT SMS Memory Command]";
        ATCommandsList.Add(new ATCommand(ATCommands.ATSetMemoryTypesUsed + $"\"{memory1TypeToUse}\",\"{memory2TypeToUse}\",\"{memory3TypeToUse}\"", ATCommands.ATEndPart));
        ATCommandsList.Add(new ATCommand(ATCommands.ATGetAvailableMemoryTypes, ATCommands.ATEndPart));
        ATCommandsList.Add(new ATCommand(ATCommands.ATGetChosenMemoryUsage, ATCommands.ATEndPart));
    }

    public override async Task<CommandProgress> Process(ModemData modemData)
    {
        try
        {
            //Give modem some breathing space. SMS is slow communication.
            await Task.Delay(ModemTimings.MS100);

            if (!modemData.Data.Contains(ATCommandsList[CommandIndex].ATCommandString))
            {
                return CommandProgress.NotExpectedDataReply;
            }
            SetModemDataForCurrentCommand(modemData);
            SendResultEvent();
            if (modemData.HasError)
            {
                return CommandProgress.Error;
            }

            if ((_memoryCommandMode == MemoryCommandMode.GetMemoryInformation && CommandIndex == 0) ||
                (_memoryCommandMode == MemoryCommandMode.SetMemoryTypesUsed && CommandIndex == 1))
            {
                ParseMemoryTypes(modemData.Data);

            }
            else if (_memoryCommandMode == MemoryCommandMode.GetMemoryInformation && CommandIndex == 1 ||
                     (_memoryCommandMode == MemoryCommandMode.SetMemoryTypesUsed && CommandIndex == 2))
            {
                ParseMemoryStats(modemData.Data);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return CommandProgress.Error;
        }

        return HasNextATCommand ? CommandProgress.NextCommand : CommandProgress.Finished;
    }

    private void ParseMemoryStats(string memoryStats)
    {
        memoryStats = memoryStats[(memoryStats.IndexOf(':') + 1)..];
        memoryStats = memoryStats[..memoryStats.IndexOf('\r')];
        var array = memoryStats.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var index = 0;
        for (var i = 0; i < array.Length; i++)
        {
            var st = array[i];

            if (i < 3)
            {
                _memoryReadTo = Memory1;
            }
            else if (i < 6)
            {
                _memoryReadTo = Memory2;
            }
            else
            {
                _memoryReadTo = Memory3;
            }

            switch (index)
            {
                case 0:
                    {
                        _memoryReadTo.MemoryType = Enum.Parse<ModemMemoryType>(st.Replace("\"", ""));
                        index++;
                        break;
                    }
                case 1:
                    {
                        _memoryReadTo.MemoryInUse = int.Parse(st);
                        index++;
                        break;
                    }
                case 2:
                    {
                        _memoryReadTo.MemoryTotal = int.Parse(st);
                        index = 0;
                        break;
                    }
            }
        }
    }

    private void ParseMemoryTypes(string memory)
    {
        //"AT+CPMS=?\r\r+CPMS: (\"ME\",\"SM\",\"MT\"),(\"ME\",\"SM\",\"MT\"),(\"SM\",\"MT\")\r\r\r\rOK\r\r"

        Memory1.MemoryTypesAvailable.Clear();
        Memory2.MemoryTypesAvailable.Clear();
        Memory3.MemoryTypesAvailable.Clear();

        memory = memory[(memory.IndexOf(':') + 1)..];
        memory = memory[..memory.IndexOf('\r')];

        //(\"ME\",\"SM\",\"MT\"),(\"ME\",\"SM\",\"MT\"),(\"SM\",\"MT\")
        var insideGroup = false;
        var insideMemory = false;
        var memoryTypeIndex = 0;
        var buffer = "";
        foreach (var c in memory)
        {
            switch (c)
            {
                case '(':
                    {
                        insideGroup = true;
                        break;
                    }
                case ')':
                    {
                        insideGroup = false;
                        memoryTypeIndex++;
                        break;
                    }
                case '"':
                    {
                        insideMemory = insideGroup && !insideMemory;
                        if (!insideMemory)
                        {
                            switch (memoryTypeIndex)
                            {
                                case 0:
                                    {
                                        Memory1.MemoryTypesAvailable.Add(Enum.Parse<ModemMemoryType>(buffer));
                                        break;
                                    }
                                case 1:
                                    {
                                        Memory2.MemoryTypesAvailable.Add(Enum.Parse<ModemMemoryType>(buffer));
                                        break;
                                    }
                                case 2:
                                    {
                                        Memory3.MemoryTypesAvailable.Add(Enum.Parse<ModemMemoryType>(buffer));
                                        break;
                                    }
                            }
                            buffer = "";
                        }
                        break;
                    }
                default:
                    {
                        if (insideGroup && insideMemory)
                        {
                            buffer += c;
                        }
                        break;
                    }
            }
        }
    }

    private void SetLists()
    {
        Memory1.ModemId = Modem.ModemId;
        Memory2.ModemId = Modem.ModemId;
        Memory3.ModemId = Modem.ModemId;
        ModemMemoryList.Add(Memory1);
        ModemMemoryList.Add(Memory2);
        ModemMemoryList.Add(Memory3);
    }
}