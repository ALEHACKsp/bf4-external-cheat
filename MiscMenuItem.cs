using System.Collections.Generic;

namespace BF4_Private_By_Tejisav
{
    internal class MiscMenuItems
    {
        private List<StoreMenuItem> Items;
        public List<StoreMenuItem> MenuIndex
        {
            get
            {
                return Items;
            }
        }
        public StoreMenuItem this[Overlay.MiscMenuList menuitems]
        {
            get
            {
                return MenuIndex[(int)menuitems];
            }
        }
        public MiscMenuItems()
        {
            Items = new List<StoreMenuItem>();
        }
        public MiscMenuItems(List<StoreMenuItem> data)
        {
            Items = data;
        }
        public void ClearList()
        {
            Items.Clear();
        }
    }
}