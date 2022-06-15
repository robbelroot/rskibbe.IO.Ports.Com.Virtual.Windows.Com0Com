

# Description
A .NET wrapper package for the com0com null modem emulator binary. It helps creating, removing and listing virtual ports by using the com0com console app. The package is based on another package called [rskibbe.IO.Ports.Com](https://www.nuget.org/packages/rskibbe.IO.Ports.Com) which provides the basic infrastructure.

# Getting started
After installation, just go ahead and import the corresponding namespace:

**C#**

    using rskibbe.IO.Ports.Com.Com0Com;
    
**Visual Basic .NET**
    
    Imports rskibbe.IO.Ports.Com.Com0Com

> Keep in mind to actually **download & install the com0com binary**, otherwise you won't be able to use it through .NET software / code. Of course it has to be installed on the pc where your app is running, too. Maybe I'll add some things to make this process easier, for now it has to be done manually.

> You can easily find the com0com download inside one of my [blog posts](https://robbelroot.de/blog/virtuelle-com-ports-mit-com0com-erstellen-emulieren) or search it on the web. Even if it's german, just **go to Downloads** and install the right setup matching your architecture.

Now you can create and use an instance of the **Coc-Class** to work with virtual COM ports.
> Also keep in mind to set the **InstallationFolder** property to tell Coc, where the binary files are actually located.
> 
> The default value is **"C:\Program Files (x86)\com0com"**. You can change it easily as:

    var myCocInstance = new Coc();
    myCocInstance.InstallationFolder = "D:\MyOtherFolder\com0com";

## Functions & Methods
Here you will find a basic overview of the available functions & methods provided by the package.

### CreateVirtualPortsAsync()
Creates a virtual port pair based on the next free COM port ids.  Needs to internally lookup the returned registration id of the console app. Returns a corresponding registration object, which contains basic information about the registration. Throws a ComPortsRegistrationException if it failed.
> Creating virtual ports always results in a pair being created as a connection always consists of two endpoints.

### CreateVirtualPortsAsync(byte portIdA, byte portIdB)
Same as above except that it's trying to use the provided COM port ids instead of the next free ones.

### RemoveVirtualPortsByNameAsync(string portNameAOrB)
Removes a virtual port pair based on one of the port names. Throws a ComPortsRemovalException if the ports couldn't be removed or if there was no registration with the provided port name available.

### RemoveVirtualPortsByRegistrationIdAsync(int id)
Removes a virtual port pair based on the registration id obtained at creation. Throws a ComPortsRemovalException if at least one of the ports couldn't be removed.

### RemoveAllVirtualPortsAsync()
Removes all registered virtual ports – even in case of different app starts. Internally calls the listing function of registered virtual ports and iterates with removal by registration id.

### ListUsedPortNamesAsync()
Lists the used / busy port names based on Com0Com's busynames command with filter "COM?*" = all usual named COM ports.
> This is not tied to virtual ports alone!

### ListUsedPortIdsAsync()
Lists the used / busy port ids based on Com0Com's busynames command with filter "COM?*" = all usual named COM ports. COM6 and COM7 being busy would return 6 & 7.
> This is not tied to virtual ports alone!

### ListVirtualPortRegistrationsAsync()
Lists all virtual COM port registrations.

### ListUsedVirtualPortNamesAsync()
Lists all virtually registered port names based on the ListVirtualPortRegistrationsAsync function.

### ListUsedVirtualPortIdsAsync()
Lists all virtually registered port ids based on the ListVirtualPortRegistrationsAsync function.

### GetVirtualComPortsByRegistrationIdAsync(int id)
Gets a COM port name pair based on the registration id.
> Internally uses and parses the output of the com0com "listfnames" command.

## Helpers
These functions are meant to be helpers for other functions, therefore not being essential. I kept them public, as you might want to use them while issueing your own commands.

### IEnumerable<IGrouping<int, string>> GroupListOutputLinesById(IEnumerable<string> lines)
Helps grouping the output of the Com0Com list command for further processing.

### string GetComPortNameByFriendlyNameLine(string line)
**Parses** (for example) a line output by the **listfnames** command.
> Turns
> CNCA0 FriendlyName="com0com - serial port emulator (COM7)"
> into
> COM7

### ProcessStartInfo GetProcessStartInfo(string arguments)
Function for preparing a ProcessStartInfo with all needed properties based on an arguments string.
> Thought about making the process instance more reusable to safe time spawning processes - didn't implement so far.

### Task<IEnumerable<string>> GetExecutableResponseAsync(ProcessStartInfo processStartInfo)
Runs the provided ProcessStartInfo (with the com0com binary) and returns the output lines. Theres also **a string overload as well** for creating the ProcessStartInfo for you.

### Task<IEnumerable<string>> ExecuteBinaryAsync(string arguments)
Helps creating a fitting ProcessStartInfo and executing the binary in one call.