using System;
using System.Collections.Generic;
using System.Text;
using UnityMultiplayerDRPlugin.DTOs;

namespace UnityMultiplayerDRPlugin.Entities
{
    class UMProceduralShapeEntity : UMEntity
    {
        public SpawnProceduralShapeEntityClientDTO spawnData;

        public UMProceduralShapeEntity(uint id, SpawnProceduralShapeEntityClientDTO spawnData)
        {
            this.id = id;

            this.spawnData = spawnData;
            this.isProceduralShape = true;

            this.X = spawnData.position.x;
            this.Y = spawnData.position.y;
            this.Z = spawnData.position.z;

            this.rotX = spawnData.rotation.x;
            this.rotY = spawnData.rotation.y;
            this.rotZ = spawnData.rotation.z;

            this.scaleX = spawnData.scale.x;
            this.scaleY = spawnData.scale.y;
            this.scaleZ = spawnData.scale.z;
            this.velocityX = 0;
            this.velocityY = 0;
            this.velocityZ = 0;
            this.hasPhysics = false;
            
        }

        public SpawnProceduralShapeEntityServerDTO getServerSpawnDTO(uint id)
        {
            //todo: implement this paradigm in other aplicable places
            SpawnProceduralShapeEntityServerDTO serverdata = new SpawnProceduralShapeEntityServerDTO
            {
                ID = id,
                type = spawnData.type,
                NrBuildingPoints = spawnData.NrBuildingPoints,
                buildingPoints = spawnData.buildingPoints,
                position = new UMVector3(this.X, this.Y, this.Z),
                rotation = new UMVector3(this.rotX, this.rotY, this.rotZ),
                scale = new UMVector3(this.scaleX, this.scaleY, this.scaleZ),
            };

            return serverdata;
        }
    }
}
