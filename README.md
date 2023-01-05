# SMSTerminal
 Basic library and SMSWindow (test app) for handling SMS transmission using SMS Terminals/Modems.
<br/>
<br/>
Tested using Telic NT910G & CT63 over RS232. 
You can set startup parameter to the tph number you want to send to via the test application.
```SMSWindow.exe 0501234567```
<br/>
<br/>
SMSTerminal supports following encodings :
* 7-bit 
* 8-bit
* UCS2

Handles concatenated SMS. Sends and receives SMS using the PDU format.