using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.InventoryDBEntities;

namespace UnityMultiplayerDRPlugin
{
    class UMGameDBInitializer : DropCreateDatabaseAlways<UMGameDatabaseContext>
    {
        protected override void Seed(UMGameDatabaseContext context)
        {
            //Initialize item list, TODO: Refactor this into 
            IList<ItemData> Items = new List<ItemData>();
            ItemData IronItem = new ItemData() { Name = "Iron", EntityID = 25, Value = 19.00f, StackSize = ushort.MaxValue - 1, Description = "" };
            Items.Add(IronItem);

            ItemData OilItem = new ItemData() { Name = "Oil", EntityID = 25, Value = 19.00f, StackSize = ushort.MaxValue - 1, Description = "" };
            Items.Add(OilItem);

            ItemData CopperItem = new ItemData() { Name = "Copper", EntityID = 25, Value = 19.00f, StackSize = ushort.MaxValue - 1, Description = "" };
            Items.Add(CopperItem);

            context.Items.AddRange(Items);

            IList<Inventory> inventories = new List<Inventory>();
            Inventory admininventory = new Inventory() { Size = 20, Items = new List<InventoryItem>() };
            admininventory.Items.Add(new InventoryItem() { Inventory = admininventory, ItemData = CopperItem, Quantity = 10, Position = 0 });
            admininventory.Items.Add(new InventoryItem() { Inventory = admininventory, ItemData = IronItem, Quantity = 10, Position = 1 });
            inventories.Add(admininventory);

            context.Inventories.AddRange(inventories);


            IList<AccountData> Accounts = new List<AccountData>();
            AccountData AdminAccount = new AccountData()
            {
                Username = "Admin",
                Characters = new List<CharacterData>()
            };

            

            CharacterData AdminCharacter = new CharacterData()
            {
                Account = AdminAccount,
                Name = "Admins character",
                Inventory = admininventory
            };

            AdminAccount.Characters.Add(AdminCharacter);

            context.Characters.Add(AdminCharacter);
            context.Accounts.Add(AdminAccount);

            Accounts.Add(AdminAccount);

            base.Seed(context);
        }

    }
}
