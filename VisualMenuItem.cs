using System.Collections.Generic;

namespace BF4_Private_By_Tejisav
{
    internal class VisualMenuItems
    {
        private List<StoreMenuItem> Items;
        public List<StoreMenuItem> MenuIndex
        {
            get
            {
                return Items;
            }
        }
        public StoreMenuItem this[Overlay.VisualMenuList menuitems]
        {
            get
            {
                return MenuIndex[(int)menuitems];
            }
        }
        public VisualMenuItems()
        {
            Items = new List<StoreMenuItem>();
        }
        public VisualMenuItems(List<StoreMenuItem> data)
        {
            Items = data;
        }
        public void ClearList()
        {
            Items.Clear();
        }
    }
}