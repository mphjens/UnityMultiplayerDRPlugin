using System;
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
        public float Radius { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }

        public Player(ushort ID, float x, float y, float z, float radius, byte colorR, byte colorG, byte colorB)
        {
            this.ID = ID;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Radius = radius;
            this.ColorR = colorR;
            this.ColorG = colorG;
            this.ColorB = colorB;
        }
    }
}
