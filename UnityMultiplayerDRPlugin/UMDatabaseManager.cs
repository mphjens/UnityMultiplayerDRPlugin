using System;
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

        //// https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms686016.aspx
        //[DllImport("Kernel32")]
        //private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

        //// https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms683242.aspx
        //private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        //private enum CtrlType
        //{
        //    CTRL_C_EVENT = 0,
        //    CTRL_BREAK_EVENT = 1,
        //    CTRL_CLOSE_EVENT = 2,
        //    CTRL_LOGOFF_EVENT = 5,
        //    CTRL_SHUTDOWN_EVENT = 6
        //}

        //public SQLiteConnection db { get; private set; }

        //public TableQuery<ItemDTO> Items { get; private set; }
        //public TableQuery<UserDTO> Users { get; private set; }
        //public TableQuery<InventoryDTO> Inventories { get; private set; }
        //public TableQuery<InventoryItemDTO> InventoryItems { get; private set; }

        public UMDatabaseManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            // Register the handler
            //SetConsoleCtrlHandler(Handler, true);

            //bool generateTables = !File.Exists(@"UMUsers.db");
            
            //db = new SQLiteConnection(@"UMUsers.db");
            //if (generateTables)
            //{
            //    db.CreateTable<ItemDTO>();
            //    db.CreateTable<UserDTO>();
            //    db.CreateTable<InventoryDTO>();
            //    db.CreateTable<InventoryItemDTO>();
            //}

            //Items = db.Table<ItemDTO>();
            //Users = db.Table<UserDTO>();
            //Inventories = db.Table<InventoryDTO>();
            //InventoryItems = db.Table<InventoryItemDTO>();

           

        }

        //private bool Handler(CtrlType signal)
        //{
        //    switch (signal)
        //    {
        //        case CtrlType.CTRL_BREAK_EVENT:
        //        case CtrlType.CTRL_C_EVENT:
        //        case CtrlType.CTRL_LOGOFF_EVENT:
        //        case CtrlType.CTRL_SHUTDOWN_EVENT:
        //        case CtrlType.CTRL_CLOSE_EVENT:
        //            Console.WriteLine("Closing");
        //            // TODO Cleanup resources
        //            db.Close();
        //            Environment.Exit(0);
        //            return false;

        //        default:
        //            return false;
        //    }
        //}

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
