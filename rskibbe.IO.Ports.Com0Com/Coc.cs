using System.Diagnostics;

namespace rskibbe.IO.Ports.Com0Com
{
    public static class Coc
    {

        public static string InstallationFolder { get; set; }


        static Coc()
        {
            InstallationFolder = @"C:\Program Files (x86)\com0com";
        }

        public static async Task<IEnumerable<string>> ListUsedPortNamesAsync()
        {
            var processStartInfo = GetProcessStartInfo("busynames COM?*");
            var lines = await GetExecutableResponseAsync(processStartInfo);
            return lines;
        }

        /// <summary>
        /// Removes a virtual port pair by registration id
        /// </summary>
        /// <param name="id">The id returned from registration</param>
        /// <returns>Whether they could be both removed or not</returns>
        /// <remarks>
        /// Expects the "Removed xy" messages returned from console app to be in the right order = Removed CNCA5 CNCB5 NOT B then A!
        /// Suppresses eventually up-popping windows by using --silent flag
        /// </remarks>
        public static async Task<bool> RemoveVirtualPortsAsync(int id)
        {
            var processStartInfo = GetProcessStartInfo($"--silent remove {id}");
            var portARemoved = false;
            var portBRemoved = false;
            var lines = await GetExecutableResponseAsync(processStartInfo);
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
            return portARemoved && portBRemoved;
        }

        public static async Task RemoveAllVirtualPortsAsync()
        {
            var processStartInfo = GetProcessStartInfo($"--silent uninstall");
            await GetExecutableResponseAsync(processStartInfo);
        }

        public static async Task<ComPortRegistration> CreateVirtualPortsAsync()
        {
            var processStartInfo = GetProcessStartInfo("install PortName=COM# PortName=COM#");
            var lines = await GetExecutableResponseAsync(processStartInfo);
            var line = lines.First();
            var registration = ComPortRegistration.FromRegistrationLine(line);
            var comPortPair = await GetComPortsByRegistrationIdAsync(registration.Id);
            registration.ComPorts = comPortPair;
            return registration;
        }

        /// <summary>
        /// Uses the listfnames command to find the real port names to the corresponding id
        /// </summary>
        public static async Task<ComPortPair> GetComPortsByRegistrationIdAsync(int id)
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

        /// <summary>
        /// Turns the following example line
        /// CNCA0 FriendlyName="com0com - serial port emulator (COM7)"
        /// into -> COM7
        /// </summary>
        public static string GetComPortNameByFriendlyNameLine(string line)
        {
            var values = line.Split("(");
            var last = values.LastOrDefault();
            last = last.Replace(")", "");
            return last;
        }

        /// <summary>
        /// Lists all registered virtual ports
        /// </summary>
        public static async Task<List<ComPortRegistration>> ListVirtualPortRegistrationsAsync()
        {
            var list = new List<ComPortRegistration>();
            var processStartInfo = GetProcessStartInfo("list");
            var lines = await GetExecutableResponseAsync(processStartInfo);
            var idToLinesGroups = lines.GroupBy(x =>
            {
                var parts = x.Split(" ");
                var identifierPart = parts[0];
                var digits = identifierPart.Where(y => Char.IsDigit(y));
                var digitString = string.Join("", digits);
                var id = Convert.ToInt32(digitString);
                return id;
            });
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
                    var port = parameters.GetValueOrDefault("RealPortName");
                    if (i == 0)
                        comPortPair.NameA = port;
                    else
                        comPortPair.NameB = port;
                }
                registration.ComPorts = comPortPair;
                list.Add(registration);
            }
            return list;
        }

        /// <summary>
        /// Helps creating the fitting <see cref="ProcessStartInfo"/> instance
        /// </summary>
        public static ProcessStartInfo GetProcessStartInfo(string arguments)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(InstallationFolder, "setupc.exe"),
                WorkingDirectory= InstallationFolder,
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
        public static async Task<IEnumerable<string>> GetExecutableResponseAsync(ProcessStartInfo processStartInfo)
        {
            var response = new List<string>();
            using var process = Process.Start(processStartInfo);
            await process.WaitForExitAsync();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                line = line.Trim();
                response.Add(line);
            }
            return response;
        }

    }
}
