namespace rskibbe.IO.Ports.Com.Com0Com.Tests
{
    public class CocTests
    {
        protected Coc Com0Com;

        [SetUp]
        public void Setup()
        {
            // make sure you always set the right executable path
            // make sure you execute with admin rights
            // Com0Com.InstallationPath = "C:\Program Files (x86)\com0com\setupc.exe";
            Com0Com = new Coc();
        }

        [Test]
        public async Task TestCreateVirtualPortsWithFreeIds()
        {
            byte highestGenerallyAvailableComPortId = 255;
            var usedIds = (await Com0Com.ListUsedPortIdsAsync()).ToList();
            var freeIds = new List<byte>();
            byte startNumber = 1;
            for (byte i = 1; i <= 2; i++)
            {
                for (byte id = startNumber; id <= highestGenerallyAvailableComPortId; id++)
                {
                    var portIdAlreadyUsed = usedIds.Contains(id);
                    if (portIdAlreadyUsed)
                        continue;
                    freeIds.Add(id);
                    var nextStartNumber = id + 1;
                    var endAlreadyReached = nextStartNumber > 255;
                    if (endAlreadyReached)
                        throw new Exception("The test needs 2 free COM port numbers");
                    startNumber = Convert.ToByte(nextStartNumber);
                    break;
                }
            }
            if (freeIds.Count != 2)
                throw new InvalidOperationException("The test needs 2 free COM port numbers to run");
            var registeredCountBefore = usedIds.Count;
            await Com0Com.CreateVirtualPortsAsync(freeIds[0], freeIds[1]);
            usedIds = (await Com0Com.ListUsedPortIdsAsync()).ToList();
            Assert.That(usedIds.Count, Is.EqualTo(registeredCountBefore + 2));
        }

        [Test]
        public void TestCreateVirtualPortsWithTakenId()
        {
            Assert.ThrowsAsync<ComPortsRegistrationException>(async () =>
            {
                byte defaultAlreadyTakenSystemPort = 1;
                await Com0Com.CreateVirtualPortsAsync(defaultAlreadyTakenSystemPort, 2);
            });
        }

        [Test]
        public async Task TestCreateVirtualPortsWithRandomId()
        {
            await Com0Com.RemoveAllVirtualPortsAsync();
            var registration = (ComPortRegistration) await Com0Com.CreateVirtualPortsAsync();
            Assert.That(registration!.Id, Is.Not.EqualTo(-1));
            Assert.That(registration.IdentifierA, Is.EqualTo($"CNCA{registration.Id}"));
            Assert.That(registration.IdentifierB, Is.EqualTo($"CNCB{registration.Id}"));
        }

        [Test]
        public async Task TestRemoveVirtualPortsByNameWithAProvided()
        {
            await Com0Com.RemoveAllVirtualPortsAsync();
            await Com0Com.CreateVirtualPortsAsync(200, 201);
            await Com0Com.RemoveVirtualPortsByNameAsync("COM200");
        }

        [Test]
        public async Task TestRemoveVirtualPortsByNameWithBProvided()
        {
            await Com0Com.RemoveAllVirtualPortsAsync();
            await Com0Com.CreateVirtualPortsAsync(200, 201);
            await Com0Com.RemoveVirtualPortsByNameAsync("COM201");
        }

        [Test]
        public async Task TestRemoveVirtualPortsByRegistrationId()
        {
            await Com0Com.RemoveAllVirtualPortsAsync();
            var registration = (ComPortRegistration)await Com0Com.CreateVirtualPortsAsync();
            await Com0Com.RemoveVirtualPortsByRegistrationIdAsync(registration.Id);
        }

        [Test]
        public async Task TestRemoveAllVirtualPorts()
        {
            await Com0Com.RemoveAllVirtualPortsAsync();
            await Com0Com.CreateVirtualPortsAsync();
            await Com0Com.CreateVirtualPortsAsync();            
            await Com0Com.RemoveAllVirtualPortsAsync();
            var portNames = await Com0Com.ListVirtualPortRegistrationsAsync();
            Assert.That(portNames.Count(), Is.EqualTo(0));
        }

        [Test]
        public void TestGroupListOutputLinesById()
        {
            var lines = new List<string>();
            lines.Add("CNCA0 PortName=COM1");
            lines.Add("CNCB0 PortName=COM2");
            lines.Add("CNCA1 PortName=COM3");
            lines.Add("CNCB1 PortName=COM4");
            lines.Add("CNCA2 PortName=COM5");
            lines.Add("CNCB2 PortName=COM6");
            var groups = Com0Com.GroupListOutputLinesById(lines);
            Assert.That(groups.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task TestListVirtualPortRegistrations()
        {
            await Com0Com.RemoveAllVirtualPortsAsync();
            var registrationOne = await Com0Com.CreateVirtualPortsAsync();
            var registrationTwo = await Com0Com.CreateVirtualPortsAsync();
            var virtualPorts = await Com0Com.ListVirtualPortRegistrationsAsync();
            Assert.That(virtualPorts.Count, Is.EqualTo(2));
        }

    }
}