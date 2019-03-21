using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnityMultiplayerDRPlugin.Entities
{
    public class UMClient
    {
        public IClient client;
        public ushort ID { get; set; }
        public WorldData World { get; set; }

        public UMClient(IClient DarkRiftClient)
        {
            this.client = DarkRiftClient;
        }
    }
}
