using System;
using System.Collections.Generic;
using System.Text;
using UnityMultiplayerDRPlugin.Entities;

namespace UnityMultiplayerDRPlugin
{
    class WorldData
    {
        public string Name = "World";   
        public Dictionary<uint, UMEntity> Entities = new Dictionary<uint, UMEntity>();
        public uint entityIdCounter = 0;
    }
}
