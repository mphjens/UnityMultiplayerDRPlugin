﻿using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;
using UnityMultiplayerDRPlugin.InventoryDBEntities;

namespace UnityMultiplayerDRPlugin
{
    public class UMInventoryManager : Plugin
    {

        //UMDatabaseManager db;

        //Todo: Move client auth/account/character logic to seperate plugins
        public Dictionary<IClient, AccountData> clientAccounts;
        public Dictionary<IClient, CharacterData> clientCharacters;

        public Dictionary<Inventory, List<IClient>> InventoryChangeSubscriptions; // A list of clients that get messaged when an inventory changes.

        public UMInventoryManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            //clients = new Dictionary<IClient, UserDTO>();
            clientAccounts = new Dictionary<IClient, AccountData>();
            clientCharacters = new Dictionary<IClient, CharacterData>();

            InventoryChangeSubscriptions = new Dictionary<Inventory, List<IClient>>();

            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;

        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            //if(db == null)
            //   db = PluginManager.GetPluginByType<UMDatabaseManager>();

            Console.WriteLine("InventoryManager registered player");
            e.Client.MessageReceived += Client_MessageReceived;

            this.BroadcastItemList(e.Client); //Send the item list to the new client
        }



        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            e.Client.MessageReceived -= Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.GetItems)
                {
                    this.OnGetItems(sender, e);
                }
                else if (message.Tag == Tags.GetInventory)
                {
                    this.OnGetInventory(sender, e);
                }
                else if (message.Tag == Tags.LoginTag) //Move to AuthManager
                {
                    this.LoginAccount(sender, e);
                }
                else if (message.Tag == Tags.LoginCharacter) //Move to CharacterManager, this manager can perhaps handle character stats, xp and other character related things.
                {
                    this.LoginCharacter(sender, e);
                }
                else if (message.Tag == Tags.TransferInventoryItem)
                {
                    this.TransferInventoryItem(sender, e);
                }
                else if (message.Tag == Tags.SubscribeInventory)
                {
                    this.SubscribeClientToInventory(sender, e);
                }

            }
        }

        private void LoginAccount(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
                    using (var db = new UMGameDatabaseContext())
                    {
                        LoginAccountClientDTO data = message.GetReader().ReadSerializable<LoginAccountClientDTO>();
                        AccountData account = db.Accounts.Include((x) => x.Characters).Where((x) => x.Username == data.Username).FirstOrDefault();

                        this.clientAccounts.Add(e.Client, account);

                        LoginAccountServerDTO response = new LoginAccountServerDTO();
                        response.Success = account != null;
                        response.Account = account.ToDTO();

                        responseWriter.Write(response);
                        using (Message responseMessage = Message.Create(Tags.LoginTag, responseWriter))
                            e.Client.SendMessage(responseMessage, SendMode.Reliable);

                    }

                    return; //terminate
                }
            }
        }

        private void TransferInventoryItem(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
                    using (var db = new UMGameDatabaseContext())
                    {
                        TransferItemClientDTO data = message.GetReader().ReadSerializable<TransferItemClientDTO>();
                        InventoryItem invItem = db.InventoryItems.Where((x) => x.Id == data.InventoryItemID).FirstOrDefault();
                        Inventory sourceInventory = db.Inventories.Where((x) => x.Id == data.SourceInventoryID).Include((x) => x.Items).FirstOrDefault();
                        Inventory destinationInventory = db.Inventories.Where((x) => x.Id == data.DestinationInventoryID).Include((x) => x.Items).FirstOrDefault();
                        bool success = false;

                        db.SaveChanges();

                        TransferItemServerDTO response = new TransferItemServerDTO();

                        if (invItem != null && destinationInventory != null)
                        {
                            invItem.Inventory = destinationInventory;
                            invItem.Position = data.Position;
                            success = true;
                        }
                        else
                        {
                            response.Message = "Item or destination does not exsist.";
                        }

                        //TODO: Don't send all InventoryItems with this message, only invItem

                        using (DarkRiftWriter writer = DarkRift.DarkRiftWriter.Create())
                        {
                            writer.Write(new InventoryUpdateDTO() { InventoryID = destinationInventory.Id, Items = new InventoryItemDTO[] { invItem.ToDTO() } });
                            Message updateMessage = DarkRift.Message.Create(Tags.OnInventoryUpdate, writer);

                            if (InventoryChangeSubscriptions.ContainsKey(sourceInventory))
                            {
                                foreach (IClient client in InventoryChangeSubscriptions[sourceInventory])
                                {
                                    client.SendMessage(updateMessage, SendMode.Reliable);
                                }
                            }

                            if (InventoryChangeSubscriptions.ContainsKey(destinationInventory))
                            {
                                foreach (IClient client in InventoryChangeSubscriptions[destinationInventory])
                                {
                                    client.SendMessage(updateMessage, SendMode.Reliable);
                                }
                            }
                        }


                        if (InventoryChangeSubscriptions.ContainsKey(destinationInventory))
                        {
                            using (DarkRiftWriter writer = DarkRift.DarkRiftWriter.Create())
                            {
                                writer.Write(new InventoryUpdateDTO() { InventoryID = destinationInventory.Id, Items = destinationInventory.Items.Select((x) => x.ToDTO()).ToArray() });
                                Message updateMessage = DarkRift.Message.Create(Tags.OnInventoryUpdate, writer);

                                foreach (IClient client in InventoryChangeSubscriptions[destinationInventory])
                                {
                                    client.SendMessage(updateMessage, SendMode.Reliable);
                                }
                            }
                        }



                        response.Success = success;
                        response.data = data;


                        responseWriter.Write(response);
                        using (Message responseMessage = Message.Create(Tags.TransferInventoryItem, responseWriter))
                            e.Client.SendMessage(responseMessage, SendMode.Reliable);

                    }

                    return; //terminate
                }
            }
        }

        private void LoginCharacter(object sender, MessageReceivedEventArgs e)
        {
            if (!clientAccounts.ContainsKey(e.Client))
            {
                Console.WriteLine($"Client {e.Client.ID} tried to login a character with no account");
                return;
            }

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
                    using (var db = new UMGameDatabaseContext())
                    {
                        LoginCharacterClientDTO data = message.GetReader().ReadSerializable<LoginCharacterClientDTO>();
                        AccountData account = clientAccounts[e.Client];

                        CharacterData character = account.Characters.Where((x) => x.Id == data.CharacterID).FirstOrDefault();
                        if (character != null)
                        {
                            this.clientCharacters.Add(e.Client, character);
                        }


                        LoginCharacterServerDTO response = new LoginCharacterServerDTO();
                        response.Success = character != null;
                        if (character != null)
                            response.Character = character.ToDTO();

                        responseWriter.Write(response);
                        using (Message responseMessage = Message.Create(Tags.LoginCharacter, responseWriter))
                            e.Client.SendMessage(responseMessage, SendMode.Reliable);

                    }

                    return; //terminate
                }
            }
        }

        private void SubscribeClientToInventory(object sender, MessageReceivedEventArgs e)
        {
            if (!clientAccounts.ContainsKey(e.Client))
            {
                Console.WriteLine($"Client {e.Client.ID} tried to subscribe to inventory with no account");
                return;
            }

            using (Message message = e.GetMessage() as Message)
            {
                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
                    using (var db = new UMGameDatabaseContext())
                    {
                        SubscribeInventoryClientDTO data = message.GetReader().ReadSerializable<SubscribeInventoryClientDTO>();
                        Inventory Inventory = db.Inventories.Where((x) => x.Id == data.InventoryID).FirstOrDefault();


                        if (Inventory != null)
                        {
                            if (!this.InventoryChangeSubscriptions.ContainsKey(Inventory) && data.Subscribe)
                            {
                                this.InventoryChangeSubscriptions.Add(Inventory, new List<IClient>());
                            }

                            if (!this.InventoryChangeSubscriptions[Inventory].Contains(e.Client))
                            {
                                if (data.Subscribe)
                                    this.InventoryChangeSubscriptions[Inventory].Add(e.Client);
                                else
                                    this.InventoryChangeSubscriptions[Inventory].Remove(e.Client);
                            }

                        }


                        SubscribeInventoryServerDTO response = new SubscribeInventoryServerDTO();
                        response.Success = Inventory != null;
                        if (Inventory != null)
                            response.InventoryID = Inventory.Id;

                        responseWriter.Write(response);
                        using (Message responseMessage = Message.Create(Tags.SubscribeInventory, responseWriter))
                            e.Client.SendMessage(responseMessage, SendMode.Reliable);

                    }

                    return; //terminate
                }
            }
        }

        public void BroadcastItemList(IClient client)
        {
            using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
            {
                using (var db = new UMGameDatabaseContext())
                {
                    IEnumerable<ItemDTO> items = db.Items.ToList().Select((x) => x.ToDTO());

                    GetItemsServerDTO response = new GetItemsServerDTO();
                    response.Items = items.ToArray();

                    responseWriter.Write(response);
                    using (Message responseMessage = Message.Create(Tags.GetItems, responseWriter))
                       client.SendMessage(responseMessage, SendMode.Reliable);

                }
            }
        }

        public void OnGetItems(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                BroadcastItemList(e.Client);
            }
        }

        public void OnGetInventory(object sender, MessageReceivedEventArgs e)
        {


            using (Message message = e.GetMessage() as Message)
            {
                GetInventoryClientDTO data = message.GetReader().ReadSerializable<GetInventoryClientDTO>();

                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
                    using (var db = new UMGameDatabaseContext())
                    {
                        Inventory invData = db.Inventories
                                    .Include((x) => x.Items.Select((i) => i.ItemData))
                                    .Where((x) => x.Id == data.InventoryID)
                                    .FirstOrDefault();
                        if (invData != null)
                        {
                            InventoryItemDTO[] inventoryItems = invData.Items.Select((x) => x.ToDTO()).ToArray();

                            GetInventoryServerDTO response = new GetInventoryServerDTO();
                            response.Inventory = invData.ToDTO();
                            response.InventoryItems = inventoryItems;

                            responseWriter.Write(response);
                            using (Message responseMessage = Message.Create(Tags.GetInventory, responseWriter))
                                e.Client.SendMessage(responseMessage, SendMode.Reliable);

                        }
                    }

                    return; //terminate
                }
            }
        }


        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);



    }
}
