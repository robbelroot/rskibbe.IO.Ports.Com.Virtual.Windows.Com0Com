using System.Diagnostics;

namespace rskibbe.IO.Ports.Com.Com0Com
{
    public class Coc : ComPortRegisterBase
    {

        public string InstallationFolder { get; set; }


        public Coc()
        {
            InstallationFolder = @"C:\Program Files (x86)\com0com";
        }

        /// <summary>
        /// Creates a virtual port pair with unused ids
        /// Takes additional time as it needs to find out the assigned ids after registration
        /// </summary>
        /// <exception cref="ComPortsRegistrationException">If no ports free</exception>
        public override async Task<IComPortRegistration> CreateVirtualPortsAsync()
        {
            var pattern = "COM#";
            var arguments = $"--silent install PortName={pattern} PortName={pattern}";
            var lines = await ExecuteBinaryAsync(arguments);
            var anyPortFailed = lines.Any(x => x.Contains("already"));
            if (anyPortFailed)
                throw new ComPortsRegistrationException($"Couldn't register COM ports based on pattern `{pattern}`");
            var registration = ComPortRegistration.FromBinaryResponse(lines);
            var comPortPair = await GetVirtualComPortsByRegistrationIdAsync(registration.Id);
            registration.ComPorts = comPortPair;
            return registration;
        }

        /// <summary>
        /// Creates a virtual port pair based on ids
        /// </summary>
        /// <exception cref="ComPortsRegistrationException">If id named ports already in use or no ports free</exception>
        public override async Task<IComPortRegistration> CreateVirtualPortsAsync(byte portIdA, byte portIdB)
        {
            var portNameA = $"COM{portIdA}";
            var portNameB = $"COM{portIdB}";
            var arguments = $"--silent install PortName={portNameA} PortName={portNameB}";
            var lines = await ExecuteBinaryAsync(arguments);
            var anyPortFailed = lines.Any(x => x.Contains("already"));
            if (anyPortFailed)
                throw new ComPortsRegistrationException($"Couldn't register COM ports with ids {portIdA} and {portIdB}");
            var registration = ComPortRegistration.FromBinaryResponse(lines);
            registration.ComPorts = new ComPortPair(portNameA, portNameB);
            return registration;
        }

        /// <summary>
        /// Removes a port pair by providing one of the pairs port name
        /// </summary>
        /// <exception cref="ComPortsRemovalException">If no pair containing the portNameAOrB could be found and therefore not be removed. Or if one of the ports could not be removed.</exception>
        public override async Task RemoveVirtualPortsByNameAsync(string portNameAOrB)
        {
            var registrations = await ListVirtualPortRegistrationsAsync();
            var registrationToRemove = registrations.SingleOrDefault(x =>
            {
                return x.ComPorts.NameA == portNameAOrB || x.ComPorts.NameB == portNameAOrB;
            });
            if (registrationToRemove == null)
                throw new ComPortsRemovalException($"The port pair containing {portNameAOrB} could not be removed.");
            await RemoveVirtualPortsByRegistrationIdAsync(((ComPortRegistration)registrationToRemove).Id);
        }

        /// <summary>
        /// Removes a virtual port pair by registration id
        /// </summary>
        /// <param name="id">The id returned from registration</param>
        /// <exception cref="ComPortsRemovalException">If at least one of the ports could not be removed</exception>
        public override async Task RemoveVirtualPortsByRegistrationIdAsync(int id)
        {
            var portARemoved = false;
            var portBRemoved = false;
            var lines = await ExecuteBinaryAsync($"--silent remove {id}");
            foreach (var line in lines)
            {
                if (line.StartsWith($"Removed CNCA{id}"))
                {
                    portARemoved = true;
                }
                else if (line.StartsWith($"Removed CNCB{id}"))
                {
                    portBRemoved = true;
                    break;
                }
            }
            var bothPortsRemoved = portARemoved && portBRemoved;
            if (!bothPortsRemoved)
                throw new ComPortsRemovalException($"The port pair with registration id {id} could not be removed");
        }

        /// <summary>
        /// Removes all virtual ports
        /// </summary>
        /// <exception cref="ComPortsRemovalException">If at least one of the ports could not be removed</exception>
        public override async Task RemoveAllVirtualPortsAsync()
        {
            var registrations = await ListVirtualPortRegistrationsAsync();
            if (registrations.Count() == 0)
                return;
            foreach (var registration in registrations)
                await RemoveVirtualPortsByRegistrationIdAsync(((ComPortRegistration)registration).Id);
        }

        /// <summary>
        /// Lists all used port names - not only virtual
        /// </summary>
        public override async Task<IEnumerable<string>> ListUsedPortNamesAsync()
        {
            var lines = await ExecuteBinaryAsync("busynames COM*?");
            return lines;
        }

        /// <summary>
        /// Lists all used port ids - not only virtual
        /// COM3 & COM4 would be 3, 4
        /// </summary>
        public override async Task<IEnumerable<byte>> ListUsedPortIdsAsync()
        {
            var usedPortNames = await ListUsedPortNamesAsync();
            var usedPortIds = new List<byte>();
            foreach (var usedPortName in usedPortNames)
            {
                var idBeenExtracted = usedPortName.ExtractByte(out var id);
                if (idBeenExtracted)
                    usedPortIds.Add(id);
            }
            return usedPortIds;
        }

        /// <summary>
        /// Lists all virtual port registrations
        /// </summary>
        public override async Task<IEnumerable<IComPortRegistration>> ListVirtualPortRegistrationsAsync()
        {
            var list = new List<ComPortRegistration>();
            var lines = await ExecuteBinaryAsync("list");
            var idToLinesGroups = GroupListOutputLinesById(lines);
            foreach (var idToLines in idToLinesGroups)
            {
                var registration = new ComPortRegistration(idToLines.Key);
                var comPortPair = new ComPortPair();
                for (var i = 0; i <= 1; i++)
                {
                    var paramsString = idToLines.ElementAt(i).Split(" ").LastOrDefault();
                    var paramsValues = paramsString.Split(",");
                    var parameters = new Dictionary<string, string>();
                    foreach (var paramsValue in paramsValues)
                    {
                        var values = paramsValue.Trim().Split("=");
                        var paramName = values[0];
                        var paramValue = values[1];
                        parameters.Add(paramName, paramValue);
                    }
                    var realPortNameAvailable = parameters.TryGetValue("RealPortName", out var realPortName);
                    if (realPortNameAvailable)
                    {
                        if (i == 0)
                            comPortPair.NameA = realPortName;
                        else
                            comPortPair.NameB = realPortName;
                        continue;
                    }
                    var portNameAvailable = parameters.TryGetValue("PortName", out var portName);
                    if (portNameAvailable)
                    {
                        if (i == 0)
                            comPortPair.NameA = portName;
                        else
                            comPortPair.NameB = portName;
                        continue;
                    }
                }
                registration.ComPorts = comPortPair;
                list.Add(registration);
            }
            return list;
        }

        /// <summary>
        /// Lists used virtual ports names like COM3, COM4
        /// </summary>
        public override async Task<IEnumerable<string>> ListUsedVirtualPortNamesAsync()
        {
            var usedVirtualPortNames = new List<string>();
            var registrations = await ListVirtualPortRegistrationsAsync();
            foreach (var registration in registrations)
                usedVirtualPortNames.AddRange(registration.ComPorts.ToNameArray());
            return usedVirtualPortNames;
        }

        /// <summary>
        /// Lists used virtual port ids like COM3, COM4 -> 3, 4
        /// </summary>
        public override async Task<IEnumerable<byte>> ListUsedVirtualPortIdsAsync()
        {
            var usedVirtualPortIds = new List<byte>();
            var registrations = await ListVirtualPortRegistrationsAsync();
            foreach (var registration in registrations)
                usedVirtualPortIds.AddRange(registration.ComPorts.ToIdArray());
            return usedVirtualPortIds;
        }

        /// <summary>
        /// Uses the listfnames command to find the real port names to the corresponding id
        /// </summary>
        public async Task<ComPortPair> GetVirtualComPortsByRegistrationIdAsync(int id)
        {
            var comPortPair = new ComPortPair();
            var processStartInfo = GetProcessStartInfo("listfnames");
            var lines = await GetExecutableResponseAsync(processStartInfo);
            foreach (var line in lines)
            {
                if (line.StartsWith($"CNCA{id}"))
                {
                    comPortPair.NameA = GetComPortNameByFriendlyNameLine(line);
                }
                else if (line.StartsWith($"CNCB{id}"))
                {
                    comPortPair.NameB = GetComPortNameByFriendlyNameLine(line);
                    return comPortPair;
                }
            }
            return comPortPair;
        }

        public IEnumerable<IGrouping<int, string>> GroupListOutputLinesById(IEnumerable<string> lines)
        {
            var grouping = lines.GroupBy(line =>
            {
                var parts = line.Split(" ");
                var identifierPart = parts[0];
                identifierPart.ExtractInt(out var id);
                return id;
            });
            return grouping;
        }

        /// <summary>
        /// Turns the following example line
        /// CNCA0 FriendlyName="com0com - serial port emulator (COM7)"
        /// into -> COM7
        /// </summary>
        public string GetComPortNameByFriendlyNameLine(string line)
        {
            var values = line.Split("(");
            if (values.Length > 0)
            {
                var last = values.Last();
                last = last.Replace(")", "");
                return last;
            }
            return string.Empty;
        }

        /// <summary>
        /// Helps creating the fitting <see cref="ProcessStartInfo"/> instance
        /// </summary>
        public ProcessStartInfo GetProcessStartInfo(string arguments)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(InstallationFolder, "setupc.exe"),
                WorkingDirectory = InstallationFolder,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return processStartInfo;
        }

        /// <summary>
        /// Helper function running the executable and returning its results
        /// </summary>
        public async Task<IEnumerable<string>> GetExecutableResponseAsync(ProcessStartInfo processStartInfo)
        {
            var response = new List<string>();
            using var process = Process.Start(processStartInfo);
            await process!.WaitForExitAsync();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                line = line!.Trim();
                response.Add(line);
            }
            return response;
        }

        /// <summary>
        /// Helper function running the executable and returning its results
        /// </summary>
        public async Task<IEnumerable<string>> ExecuteBinaryAsync(string arguments)
        {
            var processStartInfo = GetProcessStartInfo(arguments);
            var response = await GetExecutableResponseAsync(processStartInfo);
            return response;
        }

    }
}
