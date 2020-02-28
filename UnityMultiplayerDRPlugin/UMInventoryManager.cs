using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;


namespace UnityMultiplayerDRPlugin
{
    public class UMInventoryManager : Plugin
    {

        UMDatabaseManager db;
        //Todo: maybe make this an array with a fixed number of slots for the server
        public Dictionary<IClient, UserDTO> clientUsers;

        public UMInventoryManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            
            
            //clients = new Dictionary<IClient, UserDTO>();
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
            
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if(db == null)
                db = PluginManager.GetPluginByType<UMDatabaseManager>();

            Console.WriteLine("InventoryManager registered player");
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            //this.clients.Remove(e.Client);
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.GetItems)
                {
                    this.OnGetInventory(sender, e);
                }
                else if (message.Tag == Tags.GetInventory)
                {
                    this.OnGetInventory(sender, e);
                }

            }
        }

        public void OnGetItems(object sender, MessageReceivedEventArgs e)
        {

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {

                    ItemDTO[] items = null;// db.Items.ToArray();

                    GetItemsServerDTO response = new GetItemsServerDTO();
                    response.Items = items;

                    responseWriter.Write(response);
                    using (Message responseMessage = Message.Create(Tags.GetInventory, responseWriter))
                        e.Client.SendMessage(responseMessage, SendMode.Reliable);



                    return; //terminate
                }
            }
        }

        public void OnGetInventory(object sender, MessageReceivedEventArgs e)
        {

            using (Message message = e.GetMessage() as Message)
            {
                GetInventoryClientDTO data = message.GetReader().ReadSerializable<GetInventoryClientDTO>();

                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
                    InventoryDTO invData = null;/*db.Inventories
                                    .Where((x) => x.Id == data.InventoryID)
                                    .FirstOrDefault();*/
                    if (invData != null)
                    {
                        InventoryItemDTO[] inventoryItems = null; /* db.InventoryItems
                                    .Where((x) => x._inventoryid == data.InventoryID)
                                    .ToArray();*/

                        GetInventoryServerDTO response = new GetInventoryServerDTO();
                        response.InventoryID = invData.Id;
                        response.Size = invData.Size;
                        response.InventoryItems = inventoryItems;

                        responseWriter.Write(response);
                        using (Message responseMessage = Message.Create(Tags.GetInventory, responseWriter))
                            e.Client.SendMessage(responseMessage, SendMode.Reliable);

                    }

                    return; //terminate
                }
            }
        }


        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);



    }
}
