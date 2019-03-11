using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    public class UMPlayerManager : Plugin
    {
        public Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();


        public UMPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.PlayerUpdateTag)
                {
                    PlayerUpdateMessageRecieved(sender, e);
                }
                else if (message.Tag == Tags.SpawnPlayerTag)
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
                PlayerSpawnServerDTO spawnData = new PlayerSpawnServerDTO();
                foreach (Player player in players.Values)
                {
                    spawnData.ID = player.ID;
                    spawnData.entityID = player.entityId;
                    spawnData.x = player.X;
                    spawnData.y = player.Y;
                    spawnData.z = player.Z;
                    spawnData.health = player.MaxHealth;

                    playerWriter.Write(spawnData); // TODO: this may need to be created in the forloop
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
                        PlayerSpawnClientDTO clientSpawnData = reader.ReadSerializable<PlayerSpawnClientDTO>();
                        ushort entityId = clientSpawnData.entityID;
                        bool isNewPlayer = !players.ContainsKey(e.Client);
                        Player player;

                        //TODO: request to spawn at position

                        if (isNewPlayer)
                        {
                            player = new Player(
                               e.Client.ID, //Player id
                               0, 10, 0, //Position x,y,z
                               entityId,
                               100f // MaxHealth
                            );

                            players.Add(e.Client, player);
                        }
                        else
                        {   
                            player = players[e.Client];

                            player.X = 0;
                            player.Y = 10;
                            player.Z = 0;
                        }

                        Console.WriteLine($"{player.ID} ({e.Client.ID}) requested spawn. isNewPlayer {isNewPlayer}");

                        using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
                        {
                            PlayerSpawnServerDTO spawnData = new PlayerSpawnServerDTO();
                            spawnData.ID = player.ID;
                            spawnData.entityID = player.entityId;
                            spawnData.x = player.X;
                            spawnData.y = player.Y;
                            spawnData.z = player.Z;
                            spawnData.health = player.MaxHealth;

                            newPlayerWriter.Write(spawnData);

                            using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                            {
                                foreach (IClient client in ClientManager.GetAllClients())
                                    client.SendMessage(newPlayerMessage, SendMode.Reliable);
                            }

                            
                        }
                    }


                }
            }
        }

        private void PlayerUpdateMessageRecieved(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.PlayerUpdateTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        PlayerUpdateClientDTO playerUpdateData = reader.ReadSerializable<PlayerUpdateClientDTO>();
                        Player player = players[e.Client];

                        player.X = playerUpdateData.x;
                        player.Y = playerUpdateData.y;
                        player.Z = playerUpdateData.z;

                        player.RX = playerUpdateData.rx;
                        player.RY = playerUpdateData.ry;
                        player.RZ = playerUpdateData.rz;

                        player.VX = playerUpdateData.vx;
                        player.VY = playerUpdateData.vy;
                        player.VZ = playerUpdateData.vz;

                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            PlayerUpdateServerDTO playerUpdateOutData = new PlayerUpdateServerDTO();
                            playerUpdateOutData.ID = player.ID;

                            playerUpdateOutData.x = player.X;
                            playerUpdateOutData.y = player.Y;
                            playerUpdateOutData.z = player.Z;

                            playerUpdateOutData.rx = player.RX;
                            playerUpdateOutData.ry = player.RY;
                            playerUpdateOutData.rz = player.RZ;

                            playerUpdateOutData.vx = player.VX;
                            playerUpdateOutData.vy = player.VY;
                            playerUpdateOutData.vz = player.VZ;

                            writer.Write(playerUpdateOutData);

                            using (Message playerUpdateMessage = Message.Create(Tags.PlayerUpdateTag, writer))
                            {
                                foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                                    c.SendMessage(playerUpdateMessage, e.SendMode);
                            }
                        }


                    }
                }
            }
        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
