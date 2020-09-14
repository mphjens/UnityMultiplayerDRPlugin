using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    public class UMLootManager : Plugin
    {
        public Dictionary<IClient, UMClient> clients;
        private UMWorldManager worldManager;

        public UMLootManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            clients = new Dictionary<IClient, UMClient>();
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
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                
            }
            
        }

        public override Command[] Commands => new Command[]
        {
            
        };


        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
