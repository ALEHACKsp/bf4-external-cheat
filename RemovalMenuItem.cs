using System.Collections.Generic;

namespace BF4_Private_By_Tejisav
{
    internal class RemovalMenuItems
    {
        private List<StoreMenuItem> Items;
        public List<StoreMenuItem> MenuIndex
        {
            get
            {
                return Items;
            }
        }
        public StoreMenuItem this[Overlay.RemovalMenuList menuitems]
        {
            get
            {
                return MenuIndex[(int)menuitems];
            }
        }
        public RemovalMenuItems()
        {
            Items = new List<StoreMenuItem>();
        }
        public RemovalMenuItems(List<StoreMenuItem> data)
        {
            Items = data;
        }
        public void ClearList()
        {
            Items.Clear();
        }
    }
}