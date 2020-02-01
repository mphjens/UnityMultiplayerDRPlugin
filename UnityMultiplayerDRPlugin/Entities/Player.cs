using System;
using System.Collections.Generic;
using System.Text;

namespace UnityMultiplayerDRPlugin.Entities
{
    public class Player
    {
        public ushort ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float RX { get; set; }
        public float RY { get; set; }
        public float RZ { get; set; }
        public float VX { get; set; }
        public float VY { get; set; }
        public float VZ { get; set; }
        public ushort entityId { get; set; }
        public float MaxHealth = 100f;
        public bool IsAI;

        public ushort WeaponEntityID = ushort.MaxValue;

        public Player(ushort ID, float x, float y, float z, ushort entityId, float maxhealth, bool isAI)
        {
            this.ID = ID;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.entityId = entityId;
            this.MaxHealth = maxhealth;
            this.IsAI = isAI;
        }
    }
}
