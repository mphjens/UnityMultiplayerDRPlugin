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
        public DbSet<ItemData> Items { get; private set; }
        public DbSet<AccountData> Accounts { get; private set; }
        public DbSet<CharacterData> Characters { get; private set; }

        public DbSet<InventoryItemDTO> InventoryItems { get; private set; }


        public UMGameDatabaseContext() : base(@"Data Source=JENSPC\SQLEXPRESS;Initial Catalog=UMGame;Integrated Security=True")
        {
           Database.SetInitializer<UMGameDatabaseContext>(new UMGameDBInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Adds configurations for Student from separate class
            //modelBuilder.Configurations.Add(new StudentConfigurations());

            //modelBuilder.Entity<Teacher>()
            //    .ToTable("TeacherInfo");

            //modelBuilder.Entity<Teacher>()
            //    .MapToStoredProcedures();
        }


    }

}
