namespace rskibbe.IO.Ports.Com0Com.Tests
{
    public class Com0ComTests
    {
        [SetUp]
        public void Setup()
        {
            // make sure you always set the right executable path
            // make sure you execute with admin rights
            // Coc.InstallationPath = "C:\Program Files (x86)\com0com\setupc.exe";
        }

        [Test]
        public async Task TestCreateVirtualPortsWithRandomId()
        {
            var registration = await Coc.CreateVirtualPortsAsync();
            Assert.That(registration!.Id, Is.Not.EqualTo(-1));
            Assert.That(registration.IdentifierA, Is.EqualTo($"CNCA{registration.Id}"));
            Assert.That(registration.IdentifierB, Is.EqualTo($"CNCB{registration.Id}"));
            Assert.Pass();
        }

        [Test]
        public async Task TestRemoveVirtualPorts()
        {
            var registration = await Coc.CreateVirtualPortsAsync();
            var removed = await Coc.RemoveVirtualPortsAsync(registration.Id);
            Assert.That(removed, Is.EqualTo(true));
            Assert.Pass();
        }

        [Test]
        public async Task TestRemoveAllVirtualPorts()
        {
            await Coc.RemoveAllVirtualPortsAsync();
            Assert.Pass();
        }

        [Test]
        public async Task TestListVirtualPortRegistrations()
        {
            await Coc.RemoveAllVirtualPortsAsync();
            var registrationOne = await Coc.CreateVirtualPortsAsync();
            var registrationTwo = await Coc.CreateVirtualPortsAsync();
            var virtualPorts = await Coc.ListVirtualPortRegistrationsAsync();
            Assert.That(virtualPorts.Count, Is.EqualTo(2));
            Assert.Pass();
        }

    }
}