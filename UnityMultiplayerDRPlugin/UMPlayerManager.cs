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

        UMWorldManager WorldManager;

        UMWeaponManager WeaponManager;
        

        public UMPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            //This is now handled by Register/Unregister Player, the Connected/Disconnected Events are hooked up in UMWorldManager which call these methods.
            //ClientManager.ClientConnected += ClientConnected;
            //ClientManager.ClientDisconnected += ClientDisconnected;
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

        public void UnregisterPlayer(object sender, ClientDisconnectedEventArgs e)
        {
            WorldData world = WorldManager.clients[e.Client].World;
            world.players.Remove(e.Client);

            WeaponManager.UnRegisterPlayer(sender, e);

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);

                using (Message message = Message.Create(Tags.DespawnPlayerTag, writer))
                {
                    foreach (IClient client in world.GetClients())
                        client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        public void RegisterPlayer(object sender, MessageReceivedEventArgs e, WorldData World)
        {
            if(WorldManager == null)
            {
                WorldManager = PluginManager.GetPluginByType<UMWorldManager>();
            }
            if(WeaponManager == null)
            {
                WeaponManager = PluginManager.GetPluginByType<UMWeaponManager>();
            }

            WorldManager.clients[e.Client].World = World;

            e.Client.MessageReceived += Client_MessageReceived;

            //Send exsisting players
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                PlayerSpawnServerDTO spawnData = new PlayerSpawnServerDTO();
                foreach (Player player in World.players.Values)
                {
                    spawnData.ID = player.ID;
                    spawnData.entityID = player.entityId;
                    spawnData.x = player.X;
                    spawnData.y = player.Y;
                    spawnData.z = player.Z;
                    spawnData.health = player.MaxHealth;

                    playerWriter.Write(spawnData);
                }

                using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }

            //Register WeaponManager
            WeaponManager.RegisterClient(sender, e);
        }

        private void SpawnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.SpawnPlayerTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        WorldData World = WorldManager.clients[e.Client].World;
                        PlayerSpawnClientDTO clientSpawnData = reader.ReadSerializable<PlayerSpawnClientDTO>();
                        ushort entityId = clientSpawnData.entityID;
                        bool isNewPlayer = !World.players.ContainsKey(e.Client);
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

                            World.players.Add(e.Client, player);
                        }
                        else
                        {   
                            player = World.players[e.Client];

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
                                foreach (IClient client in World.GetClients())
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
                        WorldData World = WorldManager.clients[e.Client].World;
                        PlayerUpdateClientDTO playerUpdateData = reader.ReadSerializable<PlayerUpdateClientDTO>();
                        Player player = World.players[e.Client];

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

                            playerUpdateOutData.triggerQueue = playerUpdateData.triggerQueue;

                            writer.Write(playerUpdateOutData);

                            using (Message playerUpdateMessage = Message.Create(Tags.PlayerUpdateTag, writer))
                            {
                                foreach (IClient c in World.GetClients().Where(x => x != e.Client))
                                    c.SendMessage(playerUpdateMessage, e.SendMode);
                            }
                        }


                    }
                }
            }
        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 2);

    }
}
