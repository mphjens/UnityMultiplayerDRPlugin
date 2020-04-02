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
    public class UMGameDatabaseContext : DbContext
    {
        public DbSet<ItemData> Items { get; set; }
        public DbSet<AccountData> Accounts { get; set; }
        public DbSet<CharacterData> Characters { get; set; }

        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<InventoryItem> InventoryItems { get; set; }

        public UMGameDatabaseContext() : base()
        {
            Database.SetInitializer<UMGameDatabaseContext>(new UMGameDBInitializer());
        }


        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    //Adds configurations for Student from separate class
        //    //modelBuilder.Configurations.Add(new StudentConfigurations());

        //    //modelBuilder.Entity<Teacher>()
        //    //    .ToTable("TeacherInfo");

        //    //modelBuilder.Entity<Teacher>()
        //    //    .MapToStoredProcedures();
        //}


    }

}
