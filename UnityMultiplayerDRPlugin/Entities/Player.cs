﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UnityMultiplayerDRPlugin.Entities
{
    class Player
    {
        public ushort ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float RX { get; set; }
        public float RY { get; set; }
        public float RZ { get; set; }
        public ushort entityId { get; set; }
        public float Health = 100f;

        public Player(ushort ID, float x, float y, float z, ushort entityId, float health)
        {
            this.ID = ID;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.entityId = entityId;
            this.Health = health;
        }
    }
}
