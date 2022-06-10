namespace rskibbe.IO.Ports.Com0Com
{
    /// <summary>
    /// Simple structure representing a COMX <> COMY pair
    /// </summary>
    public struct ComPortPair
    {

        public string NameA { get; set; }

        public string NameB { get; set; }

        public bool IsComplete => !string.IsNullOrWhiteSpace(NameA) && !string.IsNullOrWhiteSpace(NameB);

        public ComPortPair()
        {
            NameA = string.Empty;
            NameB = string.Empty;
        }

        public ComPortPair(string nameA, string nameB)
        {
            NameA = nameA;
            NameB = nameB;
        }

        public override string ToString()
            => $"{NameA}<>{NameB}";

    }
}
