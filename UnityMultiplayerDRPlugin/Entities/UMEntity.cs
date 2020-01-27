using System;
using System.Collections.Generic;
using System.Text;
using UnityMultiplayerDRPlugin.DTOs;

namespace UnityMultiplayerDRPlugin.Entities
{
    public class UMEntity
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

        public WorldData world;
        public bool ShouldSerializeworld() { return false; }//This tells newtonsoft.json to not serialize the field

        public void WriteSpawn(DarkRift.DarkRiftWriter writer)
        {
            SpawnEntityServerDTO dto = new SpawnEntityServerDTO();
            dto.ID = id;
            dto.EntityId = entityId;
            dto.State = state;
            dto.hasPhysics = hasPhysics;
            dto.position = new UMVector3(X, Y, Z);
            dto.rotation = new UMVector3(rotX, rotY, rotZ);
            dto.scale = new UMVector3(scaleX, scaleY, scaleZ);


            writer.Write(dto);
            
        }
    }
}
