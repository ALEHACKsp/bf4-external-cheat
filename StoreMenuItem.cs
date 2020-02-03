namespace BF4_Private_By_Tejisav
{
    internal class StoreMenuItem
    {
        public int Value;
        public bool Enabled;
        public string ItemName
        {
            get;
            private set;
        }
        public bool SubItem
        {
            get;
            private set;
        }
        public string[] ValueNames
        {
            get;
            private set;
        }
        public ItemType itemtype
        {
            get;
            private set;
        }
        public StoreMenuItem(bool value, string name, bool subItem)
        {
            Enabled = value;
            ItemName = name;
            SubItem = subItem;
            itemtype = ItemType.Boolean;
        }
        public StoreMenuItem(int value, string[] valueNames, string name, bool subItem)
        {
            Value = value;
            ValueNames = valueNames;
            if (ValueNames == null)
            {
                ValueNames = new string[]
			{
				"Empty"
			};
            }
            if (Value >= ValueNames.Length)
            {
                Value = 0;
            }
            ItemName = name;
            SubItem = subItem;
            itemtype = ItemType.Integer;
        }
    }
}