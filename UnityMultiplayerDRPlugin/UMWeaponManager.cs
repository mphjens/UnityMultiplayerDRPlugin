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

        public UMWeaponManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected; ;
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.WeaponFireStartTag || message.Tag == Tags.WeaponFireEndTag || message.Tag == Tags.WeaponActionTag)
                {
                    WeaponUpdateMessageRecieved(sender, e);
                }
                else if (message.Tag == Tags.WeaponSwitchTag)
                {
                    WeaponSwitchMessageRecieved(sender, e);
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
                        WeaponSwitchClientDTO data = reader.ReadSerializable<WeaponSwitchClientDTO>();

                        using (DarkRiftWriter weaponSwitchWriter = DarkRiftWriter.Create())
                        {
                            WeaponSwitchServerDTO switchData = new WeaponSwitchServerDTO();
                            switchData.playerId = e.Client.ID;
                            switchData.weaponEntityId = data.weaponEntityId;
                            switchData.weaponSlot = data.weaponSlot;

                            weaponSwitchWriter.Write(switchData);

                            using (Message fireStartMessage = Message.Create(Tags.WeaponSwitchTag, weaponSwitchWriter)) //Repeat the incoming tagname as all message bodies are the same
                            {
                                foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
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
                        WeaponFireClientDTO data = reader.ReadSerializable<WeaponFireClientDTO>();

                        using (DarkRiftWriter fireStartWriter = DarkRiftWriter.Create())
                        {
                            WeaponFireServerDTO fireData = new WeaponFireServerDTO();
                            fireData.playerID = e.Client.ID;
                            fireData.fireNum = data.fireNum;

                            fireStartWriter.Write(fireData);

                            using (Message fireStartMessage = Message.Create(message.Tag, fireStartWriter)) //Repeat the incoming tagname as all message bodies are the same
                            {
                                foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                                    client.SendMessage(fireStartMessage, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            //Stop all firemodes for client (when sending ushort.MaxValue, by convention all firemodes are stopped)
            using (DarkRiftWriter fireStartWriter = DarkRiftWriter.Create())
            {
                WeaponFireServerDTO fireData = new WeaponFireServerDTO();
                fireData.playerID = e.Client.ID;
                fireData.fireNum = ushort.MaxValue;

                fireStartWriter.Write(fireData);

                using (Message fireStartMessage = Message.Create(Tags.WeaponFireEndTag, fireStartWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(fireStartMessage, SendMode.Reliable);
                }
            }
        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
