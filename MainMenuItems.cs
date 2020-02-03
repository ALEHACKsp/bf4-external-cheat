using System.Collections.Generic;

namespace BF4_Private_By_Tejisav
{
    internal class MainMenuItems
    {
        private List<StoreMenuItem> Items;
        public List<StoreMenuItem> MenuIndex
        {
            get
            {
                return Items;
            }
        }
        public StoreMenuItem this[Overlay.MainMenuList menuitems]
        {
            get
            {
                return MenuIndex[(int)menuitems];
            }
        }
        public MainMenuItems()
        {
            Items = new List<StoreMenuItem>();
        }
        public MainMenuItems(List<StoreMenuItem> data)
        {
            Items = data;
        }
        public void ClearList()
        {
            Items.Clear();
        }
    }
}