using System;
using System.Collections.Generic;
using System.Text;
using UnityMultiplayerDRPlugin.DTOs;

namespace UnityMultiplayerDRPlugin.Entities
{
    public class UMEntity
    {
        public uint id;
        public uint parentID;
        public bool hasPhysics;
        public ushort entityId;
        public ushort state;
        public bool isProceduralShape;

        public List<UMComponentDTO> Components;

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

        public UMEntity()
        {
            Components = new List<UMComponentDTO>();
        }

        public void WriteSpawn(DarkRift.DarkRiftWriter writer)
        {
            SpawnEntityServerDTO dto = new SpawnEntityServerDTO();
            dto.ID = id;
            dto.parentID = parentID;
            dto.EntityId = entityId;
            dto.State = state;
            dto.hasPhysics = hasPhysics;
            dto.position = new UMVector3(X, Y, Z);
            dto.rotation = new UMVector3(rotX, rotY, rotZ);
            dto.scale = new UMVector3(scaleX, scaleY, scaleZ);
            dto.components = Components.ToArray();

            writer.Write(dto);

        }

        public void RemoveComponentByID(uint componentID)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].ID == componentID)
                {
                    Components.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
