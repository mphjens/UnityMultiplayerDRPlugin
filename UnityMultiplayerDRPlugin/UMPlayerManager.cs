using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    public class UMPlayerManager : Plugin
    {
        const float MAP_WIDTH = 20;
        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();


        public UMPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    MovementMessageReceived(sender, e);
                } else if(message.Tag == Tags.SpawnPlayerTag)
                {
                    SpawnMessageReceived(sender, e);
                }

            }
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            players.Remove(e.Client);

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);

                using (Message message = Message.Create(Tags.DespawnPlayerTag, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                        client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += Client_MessageReceived;

            //Send exsisting players
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in players.Values)
                {
                    playerWriter.Write(player.ID);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Y);
                    playerWriter.Write(player.Z);
                    playerWriter.Write(player.entityId);
                    playerWriter.Write(player.Health);
                }

                using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }
        }

        private void SpawnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.SpawnPlayerTag) 
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ushort entityId = reader.ReadUInt16();
                        bool isNewPlayer = !players.ContainsKey(e.Client);

                        if (isNewPlayer)
                        {
                            Player newPlayer = new Player(
                               e.Client.ID, //Player id
                               0, 10, 0, //Position x,y,z
                               entityId,
                               100f // Health
                            );

                            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
                            {
                                newPlayerWriter.Write(newPlayer.ID);
                                newPlayerWriter.Write(newPlayer.X);
                                newPlayerWriter.Write(newPlayer.Y);
                                newPlayerWriter.Write(newPlayer.Z);
                                newPlayerWriter.Write(newPlayer.entityId);
                                newPlayerWriter.Write(newPlayer.Health);

                                using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                                {
                                    foreach (IClient client in ClientManager.GetAllClients())
                                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                                }

                                players.Add(e.Client, newPlayer);
                                
                            }
                        }
                    }

                }
            }
        }

        private void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        float newX = reader.ReadSingle();
                        float newY = reader.ReadSingle();
                        float newZ = reader.ReadSingle();

                        Player player = players[e.Client];

                        player.X = newX;
                        player.Y = newY;
                        player.Z = newZ;

                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write(player.ID);
                            writer.Write(player.X);
                            writer.Write(player.Y);
                            writer.Write(player.Z);
                            message.Serialize(writer);
                        }

                        foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                            c.SendMessage(message, e.SendMode);
                    }
                }
            }
        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
