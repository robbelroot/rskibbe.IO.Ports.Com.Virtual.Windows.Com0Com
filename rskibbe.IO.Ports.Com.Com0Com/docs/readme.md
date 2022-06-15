

# Description
A .NET wrapper package for using the com0com null modem emulator binary by .NET software.

# Getting started
After Installation, just go ahead and import the corresponding namespace:

    Imports rskibbe.IO.Ports.Com0Com

> Keep in mind to actually **download & install the com0com binary**, otherwise you won't be able to use it through .NET software. You can easily find it on one of my [blog posts](https://robbelroot.de/blog/virtuelle-com-ports-mit-com0com-erstellen-emulieren) or search it on the web. Even if it's german, just **go to Downloads** and install the right setup matching your architecture.

Now you can use the static **Coc-Class** to like create, remove and work with virtual COM ports in general.
> Also keep in mind to set the static **InstallationFolder** property to tell Coc, where the binary files are actually located. The default value is **"C:\Program Files (x86)\com0com"**. You could change it easily as:

    Coc.InstallationFolder = "D:\MyOtherFolder\com0com"

## Functions & Methods
Here you will find a basic overview of the available functions & methods provided by the package.

### CreateVirtualPortsAsync
Creates a virtual port pair by providing the naming pattern.
pattern. **"pattern" defaults to "COM#"**. Returns a corresponding registration object, which contains basic information about the registration.
> Creating virtual ports always results in a pair being created as a connection always consists of two endpoints.

### RemoveVirtualPortsAsync
Removes a virtual port pair by its registration id. Returns the informationen, where both ports could be removed successfully.
> The registration id is obtained upon registration and contained inside the registration object returned from the CreateVirtualPortsAsync function. **Suppresses** potential **popup windows** by using the --silent option.

### ListUsedPortNamesAsync
Lists the used / busy port names based on the provided pattern. **"pattern" defaults to "COM?*"**.
> This is not tied to virtual ports alone!

### RemoveAllVirtualPortsAsync
Removes all registered virtual ports.
> **Suppresses** potential **popup windows** by using the --silent option.

### GetComPortsByRegistrationIdAsync
Gets a COM port name pair based on the registration id.
> Internally uses and parses the output of the com0com "listfnames" command.

## Helpers
These functions are meant to be helpers for the other functions, therefore not essential. I kept them public, as you might want to use them while issueing your own commands.

### GetComPortNameByFriendlyNameLine
**Parses** (for example) a line output by the **listfnames** command.
> Turns
> CNCA0 FriendlyName="com0com - serial port emulator (COM7)"
> into
> COM7

### GetProcessStartInfo
Function for preparing a ProcessStartInfo with all needed properties based on an arguments string.

### GetExecutableResponseAsync
Runs the provided ProcessStartInfo (with the com0com binary) and returns the output lines. Theres also **a string overload as well** for creating the ProcessStartInfo for you.