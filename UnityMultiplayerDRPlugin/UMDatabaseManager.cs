using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;
using System.IO;
using System.Runtime.InteropServices;

namespace UnityMultiplayerDRPlugin
{
    public class UMDatabaseManager : Plugin
    {


        public UMDatabaseManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            // Register the handler
            //SetConsoleCtrlHandler(Handler, true);
            using(var db = new UMGameDatabaseContext())
            {
                db.Database.Initialize(true);
                System.Console.WriteLine("Database initialized");
            }


            //Debug: Verify initialization
            using (var db = new UMGameDatabaseContext())
            {
                db.Accounts.Include((x)=> x.Characters.Select(c => c.Inventory.Items.Select(i=>i.Inventory.Items))).ToList().ForEach((x) => {
                    System.Console.WriteLine(x.ToString());
                });
            }
        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
