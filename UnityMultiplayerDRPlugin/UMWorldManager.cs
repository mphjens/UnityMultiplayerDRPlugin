using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    public class UMWorldManager : Plugin
    {
        UMPlayerManager playerManager;
        UMEntityManager entityManager;
        UMVoipManager voipManager;

        //Todo: maybe make this an array with a fixed number of slots for the server
        public Dictionary<IClient, UMClient> clients;

        public List<WorldData> Worlds = new List<WorldData>();

        public UMWorldManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            clients = new Dictionary<IClient, UMClient>();
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (playerManager == null)
            {
                playerManager = PluginManager.GetPluginByType<UMPlayerManager>();
            }
            if (entityManager == null)
            {
                entityManager = PluginManager.GetPluginByType<UMEntityManager>();
            }
            if (voipManager == null)
            {
                voipManager = PluginManager.GetPluginByType<UMVoipManager>();
            }

            UMClient NewClient = new UMClient(e.Client);
            clients.Add(e.Client, NewClient);


            Console.WriteLine("WorldManager registered player");
            e.Client.MessageReceived += Client_MessageReceived;

            voipManager.RegisterClient(e.Client);
            
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            entityManager.UnregisterClient(e.Client);
            playerManager.UnregisterClient(e.Client);

            this.clients.Remove(e.Client);
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.JoinWorldMessage)
                {
                    this.OnJoinWorld(sender, e);
                }

                if (message.Tag == Tags.GetWorldsMessage)
                {
                    this.OnGetWorlds(sender, e);
                }

                if (message.Tag == Tags.CreateWorldMessage)
                {
                    this.OnCreateWorld(sender, e);
                }
            }
        }

        public WorldData GetWorldByName(string name)
        {
            foreach(WorldData world in Worlds)
            {
                if (world.WorldName == name)
                    return world;
            }

            return null;
        }

        public void OnCreateWorld(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                CreateWorldClientDTO data = message.GetReader().ReadSerializable<CreateWorldClientDTO>();
                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
    
                    CreateWorldServerDTO response = new CreateWorldServerDTO();
                    if(GetWorldByName(data.WorldName) == null)
                    {
                        WorldData NewWorld = new WorldData(data.WorldName, data.SceneEntityID, data.SceneName);
                        this.Worlds.Add(NewWorld);

                        response.Success = true;
                        response.Message = $"{data.WorldName} Created";
                        response.WorldData = NewWorld.ToDTO();
                        Console.WriteLine(response.Message);
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = $"{data.WorldName} already exists";
                        Console.WriteLine(response.Message);
                    }
                    

                    responseWriter.Write(response);
                    using (Message responseMessage = Message.Create(Tags.CreateWorldMessage, responseWriter))
                        e.Client.SendMessage(responseMessage, SendMode.Reliable);

                }
            }
        }

        public void OnGetWorlds(object sender, MessageReceivedEventArgs e)
        {
            using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
            {

                WorldDTO[] worldDtos = new WorldDTO[Worlds.Count];
                for (int i = 0; i < Worlds.Count; i++)
                {
                    WorldData world = Worlds[i];
                    WorldDTO worldDto = world.ToDTO();
                    worldDtos[i] = worldDto;
                }

                GetWorldsServerDTO response = new GetWorldsServerDTO();
                response.Worlds = worldDtos;

                responseWriter.Write(response);
                using (Message responseMessage = Message.Create(Tags.GetWorldsMessage, responseWriter))
                    e.Client.SendMessage(responseMessage, SendMode.Reliable);

            }
        }

        public void OnJoinWorld(object sender, MessageReceivedEventArgs e)
        {

            using (Message message = e.GetMessage() as Message)
            {
                JoinWorldClientDTO data = message.GetReader().ReadSerializable<JoinWorldClientDTO>();

                foreach (WorldData world in Worlds)
                {
                    if (world.WorldName == data.WorldName)
                    {
                        //If the player is already in a world, leave it first.
                        if(this.clients[e.Client].World != null)
                        {
                            playerManager.UnregisterClient(e.Client);
                            entityManager.UnregisterClient(e.Client);
                            this.clients[e.Client].World = null;
                        }

                        //Register the player to the world
                        this.clients[e.Client].World = world;
                        playerManager.RegisterClient(e.Client, world);
                        entityManager.RegisterClient(e.Client, world);

                        using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                        {
                            JoinWorldServerDTO response = new JoinWorldServerDTO();
                            response.Success = true;
                            response.Message = "Registered client to world";
                            Console.WriteLine(response.Message);

                            responseWriter.Write(response);
                            using (Message responseMessage = Message.Create(Tags.JoinWorldMessage, responseWriter))
                                e.Client.SendMessage(responseMessage, SendMode.Reliable);

                            return; //terminate
                        }
                    }
                }

                //World not found..
                using (DarkRiftWriter responseWriter = DarkRiftWriter.Create())
                {
                    JoinWorldServerDTO response = new JoinWorldServerDTO();
                    response.Success = false;
                    response.Message = $"No world with name: {data.WorldName}";
                    Console.WriteLine(response.Message);

                    responseWriter.Write(response);
                    using (Message responseMessage = Message.Create(Tags.JoinWorldMessage, responseWriter))
                        e.Client.SendMessage(responseMessage, SendMode.Reliable);

                }

            }

        }


        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 1);

    }
}
