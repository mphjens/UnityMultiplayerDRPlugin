using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityMultiplayerDRPlugin.DTOs;

namespace UnityMultiplayerDRPlugin.InventoryDBEntities
{
    public class ItemData
    {
        public int Id { get; set; }
        public int EntityID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public float Value { get; set; }
        public ushort StackSize { get; set; }

        public override string ToString()
        {
            return $"Item Data ({Id}): {Name}. \"{Description}\"\n";
        }

        public ItemDTO ToDTO()
        {
            var retval = new ItemDTO();
            retval.Id = Id;
            retval.EntityID = (ushort)EntityID;
            retval.Name = Name;
            retval.Description = Description;
            retval.Value = Value;
            retval.StackSize = StackSize;

            return retval;
        }
    }

    public class AccountData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public virtual ICollection<CharacterData> Characters { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Username: {Username}\n");
            if (Characters != null)
            {
                foreach (CharacterData character in Characters)
                {
                    sb.Append(character.ToString());
                }
            }

            return sb.ToString();
        }

        public AccountDataDTO ToDTO()
        {
            var retval = new AccountDataDTO();
            retval.Id = Id;
            retval.Username = Username;
            if (Characters != null)
            {
                retval.Characters = Characters.Select((x) => x.ToDTO()).ToArray();
            }



            return retval;
        }
    }

    public class CharacterData
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public virtual AccountData Account { get; set; }
        public Inventory Inventory { get; set; }
        public string Name { get; set; }
        public byte Level { get; set; }
        public float Experience { get; set; }
        public int Money { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Character: ({Id}){Name}\n");
            if (Inventory != null)
            {
                sb.Append(Inventory.ToString());
            }
            return sb.ToString();
        }

        public CharacterDataDTO ToDTO()
        {
            var retval = new CharacterDataDTO();
            retval.Id = Id;
            retval.Name = Name;
            if (Inventory != null)
                retval.InventoryID = Inventory.Id;

            retval.Level = Level;
            retval.Experience = Experience;
            retval.Money = Money;

            return retval;
        }
    }

    public class Inventory
    {
        public int Id { get; set; }

        public int Size { get; set; }
        public virtual ICollection<InventoryItem> Items { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Items != null)
            {
                sb.Append($"Inventory ({Items.Count}/{Size}):\n");
                foreach (InventoryItem item in Items)
                {
                    sb.Append(item.ToString());
                }
            }


            return sb.ToString();
        }

        public InventoryDTO ToDTO()
        {
            return new InventoryDTO() { Id = this.Id, Size = Size };
        }
    }

    public class InventoryItem
    {
        public int Id { get; set; }
        public virtual Inventory Inventory { get; set; }
        public virtual ItemData ItemData { get; set; }
        public int Quantity { get; set; }
        public int Position { get; set; }

        public override string ToString()
        {
            return $"Inventory item ({Position}): {ItemData.Name} x{Quantity}\n";
        }

        public InventoryItemDTO ToDTO()
        {
            InventoryItemDTO retval = new InventoryItemDTO() { ID = Id, InventoryID = Inventory.Id, ItemID = ItemData.Id, Position = Position, Quantity = Quantity };
            return retval;
        }
    }



    // can be used in the future
    public class InventoryCharacterPermission
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public virtual Inventory Inventory { get; set; }
        public int CharacterId { get; set; }
        public virtual CharacterData Character { get; set; }

        public enum InventoryPermission
        {
            Allowed,
            Disallowed,
        }

    }
}
