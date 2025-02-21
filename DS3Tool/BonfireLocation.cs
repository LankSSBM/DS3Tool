namespace DS3Tool
{
    public partial class MainWindow
    {
        public struct BonfireLocation
        {
            public int Offset { get; }
            public int StartBit { get; }
            public int Id { get; }

            public BonfireLocation(int offset, int startBit, int Id)
            {
                Offset = offset;
                StartBit = startBit;
                this.Id = Id;
            }
        }
    }
}
