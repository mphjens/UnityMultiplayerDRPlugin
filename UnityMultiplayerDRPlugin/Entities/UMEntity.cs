using System;
using System.Collections.Generic;
using System.Text;

namespace UnityMultiplayerDRPlugin.Entities
{
    class UMEntity
    {
        public uint id;
        public bool hasPhysics;
        public ushort entityId;
        public ushort state;
        public bool isProceduralShape;

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float rotX { get; set; }
        public float rotY { get; set; }
        public float rotZ { get; set; }

        public float scaleX { get; set; }
        public float scaleY { get; set; }
        public float scaleZ { get; set; }

        public float velocityX { get; set; }
        public float velocityY { get; set; }
        public float velocityZ { get; set; }

        public void WriteSpawn(DarkRift.DarkRiftWriter writer)
        {
            writer.Write(this.id);
            writer.Write(this.entityId);
            writer.Write(this.state);
            writer.Write(this.hasPhysics);
            writer.Write(this.X);
            writer.Write(this.Y);
            writer.Write(this.Z);
            writer.Write(this.rotX);
            writer.Write(this.rotY);
            writer.Write(this.rotZ);
            writer.Write(this.scaleX);
            writer.Write(this.scaleY);
            writer.Write(this.scaleZ);
            
        }
    }
}
