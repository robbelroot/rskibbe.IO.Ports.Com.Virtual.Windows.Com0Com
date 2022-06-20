namespace rskibbe.IO.Ports.Com.Virtual.Windows.Com0Com;

public class CocPortsRegistration : VirtualComPortRegistrationBase
{

    public int Id { get; set; }

    public string IdentifierA => $"CNCA{Id}";

    public string IdentifierB => $"CNCB{Id}";

    public CocPortsRegistration()
    {
        Id = -1;
    }

    public CocPortsRegistration(int id)
    {
        Id = id;
    }

    public static CocPortsRegistration FromBinaryResponse(IEnumerable<string> lines)
    {
        var line = lines.First();
        var lineValues = line.Split(" ");
        var firstPart = lineValues[0];
        firstPart.ExtractInt(out var id);
        var registration = new CocPortsRegistration(id);
        return registration;
    }

    public override string ToString()
        => $"{Id}: {IdentifierA}->{ComPorts.NameA}, {IdentifierB}->{ComPorts.NameB}";

}
