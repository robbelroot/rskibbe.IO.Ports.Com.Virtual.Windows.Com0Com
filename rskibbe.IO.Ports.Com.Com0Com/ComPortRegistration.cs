namespace rskibbe.IO.Ports.Com.Com0Com
{
    public class ComPortRegistration : ComPortRegistrationBase
    {

        public int Id { get; set; }

        public string IdentifierA => $"CNCA{Id}";

        public string IdentifierB => $"CNCB{Id}";

        public ComPortRegistration()
        {
            Id = -1;
        }

        public ComPortRegistration(int id)
        {
            Id = id;
        }

        public static ComPortRegistration FromBinaryResponse(IEnumerable<string> lines)
        {
            var line = lines.First();
            var lineValues = line.Split(" ");
            var firstPart = lineValues[0];
            firstPart.ExtractInt(out var id);
            var registration = new ComPortRegistration(id);
            return registration;
        }

        public override string ToString()
            => $"{Id}: {IdentifierA}->{ComPorts.NameA}, {IdentifierB}->{ComPorts.NameB}";

    }
}
