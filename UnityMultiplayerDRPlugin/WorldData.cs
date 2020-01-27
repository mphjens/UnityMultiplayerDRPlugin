using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Text;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    public class WorldData
    {
        public string WorldName { get; private set; }
        public ushort SceneEntityID { get; private set; }
        public string SceneName { get; private set; }

        public Dictionary<uint, UMEntity> Entities { get; private set; } 
        public uint entityIdCounter = 0;

        public Dictionary<IClient, Player> players { get; private set; }
        public bool ShouldSerializeplayers() { return false; } //This tells newtonsoft.json to not serialize the field

        public IClient PhysicsHost;
        public bool ShouldSerializePhysicsHost() { return false; }//This tells newtonsoft.json to not serialize the field


        public WorldData(string WorldName, ushort SceneEntityID, string SceneName)
        {
            this.WorldName = WorldName;
            this.SceneName = SceneName;
            this.SceneEntityID = SceneEntityID;
            this.Entities = new Dictionary<uint, UMEntity>();
            this.players = new Dictionary<IClient, Player>();
        }

        public void copyFields(WorldData data)
        {
            this.WorldName = data.WorldName;
            this.SceneEntityID = data.SceneEntityID;
            this.SceneName = data.SceneName;
            this.Entities = data.Entities;
            this.entityIdCounter = data.entityIdCounter;
        }

        public IEnumerable<IClient> GetClients()
        {
            return players.Keys;
        }
    }
}
