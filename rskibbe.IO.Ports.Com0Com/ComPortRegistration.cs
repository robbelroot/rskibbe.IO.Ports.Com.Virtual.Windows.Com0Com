namespace rskibbe.IO.Ports.Com0Com
{
    public class ComPortRegistration
    {

        public int Id { get; set; }

        public string IdentifierA => $"CNCA{Id}";

        public string IdentifierB => $"CNCB{Id}";

        public ComPortPair ComPorts { get; set; }

        public ComPortRegistration()
        {
            Id = -1;
        }

        public ComPortRegistration(int id)
        {
            Id = id;
        }

        public static ComPortRegistration FromRegistrationLine(string line)
        {
            var lineValues = line.Split(" ");
            var firstPart = lineValues[0];
            var digits = firstPart.Where(x => Char.IsDigit(x));
            var digitString = string.Join("", digits);
            var id = Convert.ToInt32(digitString);
            var registration = new ComPortRegistration(id);
            return registration;
        }

        public override string ToString()
            => $"{Id}: {IdentifierA}->{ComPorts.NameA}, {IdentifierB}->{ComPorts.NameB}";

    }
}
