using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using MurmurPlugin;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;
using static MurmurPlugin.VirtualServerEntity;

namespace UnityMultiplayerDRPlugin
{
    public class UMMurmurICE
    {
        IInstance serverInstance;
        IVirtualServer server;
        public bool connected;

        const string RootChannelName = "UMWorldChannels";
        int rootChannelID;

        Dictionary<string, Channel> WorldChannels; //Maps a World ID to a channel
        Dictionary<IClient, OnlineUser> ClientUsers; //Maps an IClient to a Murmur user

        public UMMurmurICE()
        {
            WorldChannels = new Dictionary<string, Channel>();
            ClientUsers = new Dictionary<IClient, OnlineUser>();
        }

        public void Connect(string secret, string IP = "127.0.0.1", int port = 6502)
        {

            // create adapter for Murmur_1.3.0.dll
            serverInstance = new MurmurAdapter.Adapter("1.3.0").Instance;
            serverInstance.Connect(IP, port, secret);

            foreach (var s in serverInstance.GetAllServers())
            {

                if (s.Value.IsRunning())
                {
                    server = s.Value;
                    break;
                }
            }

            if (server != null)
            {
                SerializableDictionary<int, VirtualServerEntity.Channel> channels = server.GetAllChannels();
                for (int i = 0; i < channels.Keys.Count; i++)
                {
                    int cID = channels.ElementAt(i).Key;
                    if(cID > 0) //We cant remove the root channel
                    {
                        server.RemoveChannel(cID);
                        Console.WriteLine("Removed channel: " + cID);
                        //i--;
                    }
                    
                }

                rootChannelID = server.AddChannel(RootChannelName, 0);

                connected = true;
                Console.WriteLine("Found Murmur server!");
            }
            else
            {
                connected = false;
                Console.WriteLine($"No Murmur server running, please start a murmur server on {IP}:{port.ToString()}");
            }

        }

        public void MoveClientToWorldChannel(IClient client, string WorldName)
        {
            this.MoveClientToChannel(client, this.WorldChannels[WorldName]);
        }


        public void MoveClientToChannel(IClient client, Channel channel)
        {
            OnlineUser clientUser = GetUser(client);
            if(clientUser != null)
            {
                clientUser.Move(server, channel.Id);
            }
        }

        public OnlineUser GetUser(IClient client)
        {
            if (ClientUsers.ContainsKey(client))
                return ClientUsers[client];

            //Find user in userlist
            foreach(OnlineUser user in server.GetOnlineUsers().Values)
            {
                if(user.Name == "UMUser_" + client.ID.ToString())
                {
                    ClientUsers.Add(client, user);
                    return user;
                }
            }

            return null;
        }

        public void RemoveUser(IClient client)
        {
            if(ClientUsers.ContainsKey(client))
                ClientUsers.Remove(client);
        }

        public void CreateChannelForWorld(string worldname)
        {
            if (!WorldChannels.ContainsKey(worldname))
            {
                int nChannelID = server.AddChannel(worldname, rootChannelID);
                Channel channel = server.GetAllChannels().Where((x) => { return x.Key == nChannelID; }).FirstOrDefault().Value;

                WorldChannels.Add(worldname, channel);

                Console.WriteLine($"Created Channel for {worldname}");
            }
        }

    }
}
