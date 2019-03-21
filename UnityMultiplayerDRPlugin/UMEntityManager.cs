using DarkRift;
using DarkRift.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityMultiplayerDRPlugin.DTOs;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    class UMEntityManager : Plugin
    {

        //WorldData World = new WorldData();
        UMWorldManager WorldManager;
        
        public UMEntityManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {

            //ClientManager.ClientConnected += ClientConnected;
            //ClientManager.ClientDisconnected += ClientDisconnected;
        }

        //Gets called before the clients are removed from the world's client list 
        public void UnRegisterPlayer(object sender, ClientDisconnectedEventArgs e)
        {
            WorldData World = WorldManager.clients[e.Client].World;
            if (e.Client == World.PhysicsHost)
            {
                World.PhysicsHost = null;
                if (World.GetClients().Count() > 1)
                {
                    this.SetPhysicsHost(World.GetClients().Where(x => x != e.Client).First());
                }
            }
        }

        public void RegisterPlayer(object sender, MessageReceivedEventArgs e, WorldData World)
        {
            if(WorldManager == null)
            {
                WorldManager = PluginManager.GetPluginByType<UMWorldManager>();
            }

            if (WorldManager.clients[e.Client].World.PhysicsHost == null)
            {
                this.SetPhysicsHost(e.Client);
            }

            BroadcastEntities(e.Client, World);
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                WorldData World = WorldManager.clients[e.Client].World;
                if (message.Tag == Tags.SpawnEntityTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        ushort newEntityId = reader.ReadUInt16();
                        ushort newState = reader.ReadUInt16();
                        bool hasPhysics = reader.ReadBoolean();
                        float newX = reader.ReadSingle();
                        float newY = reader.ReadSingle();
                        float newZ = reader.ReadSingle();

                        this.SpawnEntity(World, newEntityId, newState, hasPhysics, newX, newY, newZ);
                    }
                }
                if(message.Tag == Tags.SpawnProceduralShapeEntityTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        SpawnProceduralShapeEntityClientDTO data = reader.ReadSerializable<SpawnProceduralShapeEntityClientDTO>();
                        Console.WriteLine("Got spawn procedural message");
                        this.SpawnProceduralShapeEntity(World, data);
                    }
                }
                if (message.Tag == Tags.DespawnEntityTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        uint id = reader.ReadUInt32();
                        this.DespawnEntity(World, id);
                    }
                }

                if (message.Tag == Tags.SetStateEntityTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        uint id = reader.ReadUInt32();
                        ushort newState = reader.ReadUInt16();

                        SetState(World, id, newState);
                    }
                }

                if (message.Tag == Tags.TransformEntityTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        uint id = reader.ReadUInt32();
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();

                        float rx = reader.ReadSingle();
                        float ry = reader.ReadSingle();
                        float rz = reader.ReadSingle();

                        float sx = reader.ReadSingle();
                        float sy = reader.ReadSingle();
                        float sz = reader.ReadSingle();

                        SetTransform(World, id, x, y, z, rx, ry, rz, sx, sy, sz);
                    }
                }

                if (message.Tag == Tags.PhysicsUpdateEntityTag)
                {
                    if (e.Client == WorldManager.clients[e.Client].World.PhysicsHost)
                        PhysicsUpdate(sender, e, WorldManager.clients[e.Client].World);
                }
                if (message.Tag == Tags.SetPhysicsEntityTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        uint id = reader.ReadUInt32();
                        bool hasPhysics = reader.ReadBoolean();
                        bool isKinematic = reader.ReadBoolean();
                        setPhysics(World, id, hasPhysics, isKinematic);
                    }
                }
            }
        }

        public void BroadcastEntities(IClient client, WorldData World)
        {
            //TODO: don't loop over the entities twice here
            using (DarkRiftWriter entityWriter = DarkRiftWriter.Create())
            {
                
                foreach (UMEntity entity in World.Entities.Values)
                {
                    if (!entity.isProceduralShape)
                    {
                        entity.WriteSpawn(entityWriter);
                    }
                }

                using (Message spawnWorldEntitiesMessage = Message.Create(Tags.SpawnEntityTag, entityWriter))
                {
                    client.SendMessage(spawnWorldEntitiesMessage, SendMode.Reliable);
                }

            }

            using (DarkRiftWriter proceduralEntityWriter = DarkRiftWriter.Create())
            {

                foreach (UMEntity entity in World.Entities.Values)
                {
                    if (entity.isProceduralShape)
                    {
                        proceduralEntityWriter.Write(((UMProceduralShapeEntity)entity).getServerSpawnDTO(entity.id));
                    }
                }

                using (Message spawnWorldProceduralEntitiesMessage = Message.Create(Tags.SpawnProceduralShapeEntityTag, proceduralEntityWriter))
                {
                    client.SendMessage(spawnWorldProceduralEntitiesMessage, SendMode.Reliable);
                }

            }
        }

        public void PhysicsUpdate(object sender, MessageReceivedEventArgs e, WorldData World)
        {
            using (DarkRiftReader reader = e.GetMessage().GetReader())
            {
                using (DarkRiftWriter entityPhysWriter = DarkRiftWriter.Create())
                {
                    while (reader.Position < reader.Length)
                    {
                        uint id = reader.ReadUInt32();
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();

                        float rx = reader.ReadSingle();
                        float ry = reader.ReadSingle();
                        float rz = reader.ReadSingle();

                        float vx = reader.ReadSingle();
                        float vy = reader.ReadSingle();
                        float vz = reader.ReadSingle();

                        if (World.Entities.ContainsKey(id))
                        {
                            World.Entities[id].X = x;
                            World.Entities[id].Y = y;
                            World.Entities[id].Z = z;
                            World.Entities[id].rotX = rx;
                            World.Entities[id].rotY = ry;
                            World.Entities[id].rotZ = rz;
                            World.Entities[id].velocityX = vx;
                            World.Entities[id].velocityY = vy;
                            World.Entities[id].velocityZ = vz;


                            entityPhysWriter.Write(id);
                            entityPhysWriter.Write(x);
                            entityPhysWriter.Write(y);
                            entityPhysWriter.Write(z);

                            entityPhysWriter.Write(rx);
                            entityPhysWriter.Write(ry);
                            entityPhysWriter.Write(rz);

                            entityPhysWriter.Write(vx);
                            entityPhysWriter.Write(vy);
                            entityPhysWriter.Write(vz);

                        }

                    }


                    using (Message physUpdateMessage = Message.Create(Tags.PhysicsUpdateEntityTag, entityPhysWriter))
                    {
                        foreach (IClient c in World.GetClients().Where(cl => cl != World.PhysicsHost))
                            c.SendMessage(physUpdateMessage, SendMode.Unreliable);
                    }
                }
            }
        }

        public void setPhysics(WorldData World, uint id, bool hasPhysics, bool isKinematic)
        {

            if (World.Entities.ContainsKey(id))
            {
                World.Entities[id].hasPhysics = hasPhysics;

                using (DarkRiftWriter physSettingsWriter = DarkRiftWriter.Create())
                {
                    physSettingsWriter.Write(id);
                    physSettingsWriter.Write(hasPhysics);
                    physSettingsWriter.Write(isKinematic);

                    using (Message physUpdateMessage = Message.Create(Tags.SetPhysicsEntityTag, physSettingsWriter))
                    {
                        foreach (IClient c in World.GetClients())
                            c.SendMessage(physUpdateMessage, SendMode.Unreliable);
                    }
                }
            }

        }

        public uint SpawnProceduralShapeEntity(WorldData World, SpawnProceduralShapeEntityClientDTO data)
        {
            World.entityIdCounter++;
            uint newID = World.entityIdCounter;
            UMProceduralShapeEntity newProceduralEntity = new UMProceduralShapeEntity(newID, data);
            newProceduralEntity.spawnData = data;
            newProceduralEntity.isProceduralShape = true;

            World.Entities.Add(newID, newProceduralEntity);

            using (DarkRiftWriter proceduralEntityWriter = DarkRiftWriter.Create())
            {
                proceduralEntityWriter.Write(newProceduralEntity.getServerSpawnDTO(newID));

                using (Message spawnEntityMsg = Message.Create(Tags.SpawnProceduralShapeEntityTag, proceduralEntityWriter))
                {
                    foreach (IClient c in World.GetClients())
                        c.SendMessage(spawnEntityMsg, SendMode.Reliable);
                }

            }

            return newID;
        }

        public uint SpawnEntity(WorldData World, ushort entityId, ushort state, bool hasPhysics, float x, float y, float z,
            float rotX = 0, float rotY = 0, float rotZ = 0,
            float scaleX = 1, float scaleY = 1, float scaleZ = 1)
        {
            UMEntity newEntity = new UMEntity();

            newEntity.world = World;
            newEntity.entityId = entityId;
            newEntity.state = state;
            newEntity.hasPhysics = hasPhysics;
            newEntity.X = x;
            newEntity.Y = y;
            newEntity.Z = z;
            newEntity.rotX = rotX;
            newEntity.rotY = rotY;
            newEntity.rotZ = rotZ;
            newEntity.scaleX = scaleX;
            newEntity.scaleY = scaleY;
            newEntity.scaleZ = scaleZ;

            World.entityIdCounter++;
            newEntity.id = World.entityIdCounter;

            Console.WriteLine("COUNTER = " + World.entityIdCounter);

            World.Entities.Add(newEntity.id, newEntity);


            using (DarkRiftWriter entityWriter = DarkRiftWriter.Create())
            {
                newEntity.WriteSpawn(entityWriter);

                using (Message spawnEntityMsg = Message.Create(Tags.SpawnEntityTag, entityWriter))
                {
                    foreach (IClient c in World.GetClients())
                        c.SendMessage(spawnEntityMsg, SendMode.Reliable);
                }

            }


            return newEntity.id;
        }

        public void DespawnEntity(WorldData World, uint id)
        {
            if (World.Entities.ContainsKey(id))
            {
                World.Entities.Remove(id);

                using (DarkRiftWriter entityWriter = DarkRiftWriter.Create())
                {
                    entityWriter.Write(id);

                    using (Message despawnEntityMsg = Message.Create(Tags.DespawnEntityTag, entityWriter))
                    {
                        foreach (IClient c in World.GetClients())
                            c.SendMessage(despawnEntityMsg, SendMode.Reliable);
                    }

                }
            }
            else
            {
                Console.WriteLine("Trying to despawn non exsistant entity: " + id);
            }
        }

        public bool SetState(WorldData World, uint id, ushort state)
        {
            if (World.Entities.ContainsKey(id))
            {
                World.Entities[id].state = state;

                using (DarkRiftWriter entityStateWriter = DarkRiftWriter.Create())
                {
                    entityStateWriter.Write(id);
                    entityStateWriter.Write(state);

                    using (Message setStateMessage = Message.Create(Tags.SetStateEntityTag, entityStateWriter))
                    {
                        foreach (IClient c in World.GetClients())
                            c.SendMessage(setStateMessage, SendMode.Reliable);
                    }

                }
            }
            else
            {
                Console.WriteLine("Setting state on non exsistant entity: " + id);
            }

            return false;
        }

        public void SetPhysicsHost(IClient client)
        {
            using (DarkRiftWriter entityStateWriter = DarkRiftWriter.Create())
            {
                entityStateWriter.Write(1337);

                using (Message setHostMessage = Message.Create(Tags.SetEntityPhysicsHost, entityStateWriter))
                {
                    client.SendMessage(setHostMessage, SendMode.Reliable);
                    WorldManager.clients[client].World.PhysicsHost = client;
                }

            }
        }




        public void SetTransform(WorldData World, uint id, float x = 0, float y = 0, float z = 0, float rx = 0, float ry = 0, float rz = 0, float sx = 1, float sy = 1, float sz = 1)
        {
            if (World.Entities.ContainsKey(id))
            {
                World.Entities[id].X = x;
                World.Entities[id].Y = y;
                World.Entities[id].Z = z;
                World.Entities[id].rotX = rx;
                World.Entities[id].rotY = ry;
                World.Entities[id].rotZ = rz;
                World.Entities[id].scaleX = sx;
                World.Entities[id].scaleY = sy;
                World.Entities[id].scaleZ = sz;

                using (DarkRiftWriter entityTransformWriter = DarkRiftWriter.Create())
                {
                    entityTransformWriter.Write(id);
                    entityTransformWriter.Write(x);
                    entityTransformWriter.Write(y);
                    entityTransformWriter.Write(z);

                    entityTransformWriter.Write(rx);
                    entityTransformWriter.Write(ry);
                    entityTransformWriter.Write(rz);

                    entityTransformWriter.Write(sx);
                    entityTransformWriter.Write(sy);
                    entityTransformWriter.Write(sz);

                    using (Message setStateMessage = Message.Create(Tags.TransformEntityTag, entityTransformWriter))
                    {
                        foreach (IClient c in World.GetClients())
                            c.SendMessage(setStateMessage, SendMode.Unreliable);

                    }

                }
            }
            else
            {
                Console.WriteLine("Setting transform on non exsistant entity: " + id);
            }
        }

        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 0, 3);

        public override Command[] Commands => new Command[]
        {
            new Command("clear", "Despawns all entities from all worlds", "", ClearCommandHandler),
            new Command("save", "Saves the world as a json file", "", SaveCommandHandler),
            new Command("load", "Loads a world", "", LoadCommandHandler)
        };
        
        void SaveCommandHandler(object sender, CommandEventArgs e)
        {
            foreach(WorldData World in WorldManager.Worlds)
            {
                string json = JsonConvert.SerializeObject(World);
                if (!File.Exists(World.WorldName + ".json"))
                {
                    File.Create(World.WorldName + ".json").Close();
                }
                StreamWriter writer = new StreamWriter(World.WorldName + ".json");
                writer.Write(json);
                writer.Close();
            }
            
        }

        void LoadCommandHandler(object sender, CommandEventArgs e)
        {
            foreach (WorldData World in WorldManager.Worlds)
            {
                if(World.WorldName == e.Arguments[1])
                {
                    string filename = e.Arguments[1] + ".json";
                    if (File.Exists(filename))
                    {
                        StreamReader reader = new StreamReader(filename);
                        string json = reader.ReadToEnd();
                        reader.Close();

                        WorldData world = JsonConvert.DeserializeObject<WorldData>(json);
                        
                        foreach (IClient c in world.GetClients())
                            BroadcastEntities(c, world);
                    }
                    else
                    {
                        Console.WriteLine(filename + " not found");
                    }
                }
                
            }
        }

        void ClearCommandHandler(object sender, CommandEventArgs e)
        {
            foreach (WorldData World in WorldManager.Worlds)
            {
                uint[] keys = World.Entities.Keys.ToArray();
                foreach (uint key in keys)
                {
                    this.DespawnEntity(World, key);
                }
            }
        }
    }
}
