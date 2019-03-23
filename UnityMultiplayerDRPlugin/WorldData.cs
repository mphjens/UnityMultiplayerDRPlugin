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
        public IClient PhysicsHost;

        public WorldData(string WorldName, ushort SceneEntityID, string SceneName)
        {
            this.WorldName = WorldName;
            this.SceneName = SceneName;
            this.SceneEntityID = SceneEntityID;
            this.Entities = new Dictionary<uint, UMEntity>();
            this.players = new Dictionary<IClient, Player>();
        }

        public IEnumerable<IClient> GetClients()
        {
            return players.Keys;
        }
    }
}
