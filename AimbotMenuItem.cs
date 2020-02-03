using System.Collections.Generic;

namespace BF4_Private_By_Tejisav
{
    internal class AimbotMenuItems
    {
        private List<StoreMenuItem> Items;
        public List<StoreMenuItem> MenuIndex
        {
            get
            {
                return Items;
            }
        }
        public StoreMenuItem this[Overlay.AimbotMenuList menuitems]
        {
            get
            {
                return MenuIndex[(int)menuitems];
            }
        }
        public AimbotMenuItems()
        {
            Items = new List<StoreMenuItem>();
        }
        public AimbotMenuItems(List<StoreMenuItem> data)
        {
            Items = data;
        }
        public void ClearList()
        {
            Items.Clear();
        }
    }
}