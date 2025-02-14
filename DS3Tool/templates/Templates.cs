using System.Collections.Generic;

namespace DS3Tool.templates
{
    public class ItemTemplate
    {
        public string ItemName { get; set; }
        public string Infusion { get; set; } = "Normal";
        public string Upgrade { get; set; } = "+0";
        public uint Quantity { get; set; } = 1;
    }

    public class LoadoutTemplate
    {
        public string Name { get; set; }
        public List<ItemTemplate> Items { get; set; }
    }
}
