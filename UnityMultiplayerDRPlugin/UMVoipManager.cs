using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    public class UMVoipManager : Plugin
    {
        //Todo: maybe make this an array with a fixed number of slots for the server
        public Dictionary<IClient, UMClient> clients;

        UMWorldManager worldManager;

        UMMurmurICE MurmurServer;

        public bool isConnected()
        {
            return MurmurServer.connected;
        }

        public UMVoipManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            MurmurServer = new UMMurmurICE();
            clients = new Dictionary<IClient, UMClient>();

            MurmurServer.Connect("XOLI8WUJ06XFDX2NME41");
        }

        public void UnregisterClient(IClient client)
        {
            client.MessageReceived -= Client_MessageReceived;
        }

        public void RegisterClient(IClient client)
        {
            if(worldManager == null)
            {
                worldManager = PluginManager.GetPluginByType<UMWorldManager>();
            }

            client.MessageReceived += Client_MessageReceived;
        }



        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            this.clients.Remove(e.Client);
            MurmurServer.RemoveUser(e.Client);
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (isConnected())
                {

                    if (message.Tag == Tags.JoinWorldMessage)
                    {
                        using (DarkRiftReader reader = message.GetReader())
                        {
                            JoinWorldClientDTO data = reader.ReadSerializable<JoinWorldClientDTO>();
                            MurmurServer.MoveClientToWorldChannel(e.Client, data.WorldName); //Creates a channel for this world if it not already exists
                            
                        }
                    }

                    if (message.Tag == Tags.CreateWorldMessage)
                    {

                        using (DarkRiftReader reader = message.GetReader())
                        {
                            CreateWorldClientDTO data = reader.ReadSerializable<CreateWorldClientDTO>();
                            MurmurServer.CreateChannelForWorld(data.WorldName);
                        }
                    }
                }
            }
            
        }

        public override Command[] Commands => new Command[]
        {
            new Command("connectice", "Connect to the murmur ICE", "", ConnectIceCommandHandler),
        };

        void ConnectIceCommandHandler(object sender, CommandEventArgs e)
        {
            this.MurmurServer.Connect("XOLI8WUJ06XFDX2NME41");

        }


        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
