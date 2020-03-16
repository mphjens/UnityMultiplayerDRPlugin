using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityMultiplayerDRPlugin.InventoryDBEntities
{
    public class ItemData
    {
        public int ItemId { get; set; }
        public ushort EntityID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public float Value { get; set; }
        public ushort StackSize { get; set; }
    }

    public class AccountData
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public ICollection<CharacterData> Characters { get; set; }
    }

    public class CharacterData
    {
        public int CharacterId { get; set; }
        public virtual AccountData Account { get; set; }
        public virtual Inventory Inventory { get; set; }

        public string Name { get; set; }
        public byte Level { get; set; }
        public float Experience { get; set; }
        public int Money { get; set; }
    }

    public class Inventory
    {
        public int InventoryId { get; set; }
        public int Size { get; set; }
        public ICollection<InventoryItem> Items { get; set; }
    }

    public class InventoryItem
    {
        public int InventoryItemId { get; set; }
        public int InventoryId { get; set; }
        public virtual ItemData ItemData { get; set; }
        public int Quantity { get; set; }
        public int Position { get; set; }
    }
}
