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
    class UMGameDBInitializer : DropCreateDatabaseIfModelChanges<UMGameDatabaseContext>
    {
        protected override void Seed(UMGameDatabaseContext context)
        {
            IList<ItemData> Items = new List<ItemData>();

            Items.Add(new ItemData() { Name = "Shoe", EntityID = 99, Value = 19.00f, StackSize = 2, Description = "A single shoe." });
            Items.Add(new ItemData() { Name = "Loaf", EntityID = 99, Value = 1.12f, StackSize = 10, Description = "A loaf of bread." });
            Items.Add(new ItemData() { Name = "Money Tree", EntityID = 99, Value = 99.95f, StackSize = 1, Description = "A tree that grows money." });
            

            context.Items.AddRange(Items);

            base.Seed(context);
        }
    }
}
