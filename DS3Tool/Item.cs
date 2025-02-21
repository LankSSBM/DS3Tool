namespace DS3Tool
{
    public partial class MainWindow
    {
        public class Item
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Type { get; set; }

            public Item(string name, string address, string type = null)
            {
                Name = name;
                Address = address;
                Type = type;
            }
        }
    }
}
