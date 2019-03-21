using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    public class UMWeaponManager : Plugin
    {
        UMPlayerManager playerManager;
        UMWorldManager worldManager;

        public UMWeaponManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            //ClientManager.ClientConnected += ClientConnected;
            //ClientManager.ClientDisconnected += ClientDisconnected; 
        }

        public void RegisterClient(object sender, MessageReceivedEventArgs e)
        {
            if(playerManager == null)
            {
                playerManager = PluginManager.GetPluginByType<UMPlayerManager>();
            }

            if (worldManager == null)
            {
                worldManager = PluginManager.GetPluginByType<UMWorldManager>();
            }


            Console.WriteLine("Weaponmanager registered player");
            e.Client.MessageReceived += Client_MessageReceived;

            WorldData World = worldManager.clients[e.Client].World;
            foreach (Player p in World.players.Values)
            {
                if(p.WeaponEntityID != ushort.MaxValue)
                {
                    using (DarkRiftWriter weaponSwitchWriter = DarkRiftWriter.Create())
                    {

                        WeaponSwitchServerDTO switchData = new WeaponSwitchServerDTO();
                        switchData.playerId = e.Client.ID;
                        switchData.weaponEntityId = p.WeaponEntityID;
                        switchData.weaponSlot = 0;

                        weaponSwitchWriter.Write(switchData);
                        using (Message fireStartMessage = Message.Create(Tags.WeaponSwitchTag, weaponSwitchWriter)) //Repeat the incoming tagname as all message bodies are the same
                        {
                            e.Client.SendMessage(fireStartMessage, SendMode.Reliable);
                        }
                    }
                }
            }
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.WeaponFireStartTag || message.Tag == Tags.WeaponFireEndTag || message.Tag == Tags.WeaponActionTag)
                {
                    Console.WriteLine("Got weapon update message");
                    WeaponUpdateMessageRecieved(sender, e);
                }
                else if (message.Tag == Tags.WeaponSwitchTag)
                {
                    Console.WriteLine("Got weapon switch message");
                    WeaponSwitchMessageRecieved(sender, e);
                }
                else if (message.Tag == Tags.DamageHurtableTag)
                {
                    Console.WriteLine("Got damage hurtable message");
                    DamageHurtableRecieved(sender, e);
                }
            }
        }

        public void DamageHurtableRecieved(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.DamageHurtableTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        WorldData World = worldManager.clients[e.Client].World;
                        DamageHurtableClientDTO data = reader.ReadSerializable<DamageHurtableClientDTO>();

                        using (DarkRiftWriter damageHurtableWriter = DarkRiftWriter.Create())
                        {

                            DamageHurtableServerDTO damageHurtableData = new DamageHurtableServerDTO();
                            damageHurtableData.InstagatorID = e.Client.ID;
                            damageHurtableData.VictimID = data.VictimID;
                            damageHurtableData.damage = data.damage;

                            Console.WriteLine($"{damageHurtableData.InstagatorID} damaging {damageHurtableData.VictimID} for {damageHurtableData.damage} hp");

                            damageHurtableWriter.Write(damageHurtableData);
                            using (Message damageHurtableMessage = Message.Create(Tags.DamageHurtableTag, damageHurtableWriter))
                            {
                                foreach (IClient client in World.GetClients())
                                    client.SendMessage(damageHurtableMessage, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
        }

        public void WeaponSwitchMessageRecieved(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.WeaponSwitchTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        WorldData World = worldManager.clients[e.Client].World;
                        WeaponSwitchClientDTO data = reader.ReadSerializable<WeaponSwitchClientDTO>();
                        Player cPlayer = World.players[e.Client];
                        cPlayer.WeaponEntityID = data.weaponEntityId;



                        using (DarkRiftWriter weaponSwitchWriter = DarkRiftWriter.Create())
                        {
                            WeaponSwitchServerDTO switchData = new WeaponSwitchServerDTO();
                            switchData.playerId = e.Client.ID;
                            switchData.weaponEntityId = data.weaponEntityId;
                            switchData.weaponSlot = data.weaponSlot;

                            weaponSwitchWriter.Write(switchData);
                            using (Message fireStartMessage = Message.Create(Tags.WeaponSwitchTag, weaponSwitchWriter)) //Repeat the incoming tagname as all message bodies are the same
                            {
                                foreach (IClient client in World.GetClients().Where(x => x != e.Client))
                                    client.SendMessage(fireStartMessage, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
        }

        public void WeaponUpdateMessageRecieved(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.WeaponFireStartTag || message.Tag == Tags.WeaponFireEndTag || message.Tag == Tags.WeaponActionTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        WeaponFireClientDTO data = reader.ReadSerializable<WeaponFireClientDTO>(); //Read the weapon fire dto off the stack

                        using (DarkRiftWriter fireStartWriter = DarkRiftWriter.Create())
                        {
                            WeaponFireServerDTO fireData = new WeaponFireServerDTO();
                            fireData.playerID = e.Client.ID;
                            fireData.fireNum = data.fireNum;

                            fireStartWriter.Write(fireData);
                            int extrastart = reader.Position;
                            int extralength = reader.Length - reader.Position;
                            byte[] rawExtradata = reader.ReadRaw(extralength);
                            fireStartWriter.WriteRaw(rawExtradata, 0, extralength); //Write the extra data from the message
                            //TODO: Think about security implications of sending raw data from client to all clients

                            using (Message fireStartMessage = Message.Create(message.Tag, fireStartWriter)) //Repeat the incoming tagname as all message bodies are the same
                            {
                                foreach (IClient client in worldManager.clients[e.Client].World.GetClients().Where(x => x != e.Client))
                                    client.SendMessage(fireStartMessage, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
        }

        public void UnRegisterPlayer(object sender, ClientDisconnectedEventArgs e)
        {
            //Stop all firemodes for client (when sending ushort.MaxValue, by convention all firemodes are stopped)
            using (DarkRiftWriter fireEndWriter = DarkRiftWriter.Create())
            {
                WeaponFireServerDTO fireData = new WeaponFireServerDTO();
                fireData.playerID = e.Client.ID;
                fireData.fireNum = ushort.MaxValue;

                fireEndWriter.Write(fireData);

                using (Message fireStartMessage = Message.Create(Tags.WeaponFireEndTag, fireEndWriter))
                {
                    foreach (IClient client in worldManager.clients[e.Client].World.GetClients().Where(x => x != e.Client))
                        client.SendMessage(fireStartMessage, SendMode.Reliable);
                }
            }
        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 2);

    }
}
