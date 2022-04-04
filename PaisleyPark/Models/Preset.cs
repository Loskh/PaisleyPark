using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PaisleyPark.Models
{
    /// <summary>
    /// Preset Model for use in the application.
    /// </summary>
    public class Preset : INotifyPropertyChanged
    {
        /// <summary>
        /// Name of this preset.
        /// </summary>
        public string Name { get; set; }
        public ushort MapID { get; set; }
        /// <summary>
        /// Waymark values for all of every waymark in the game.
        /// </summary>
        public Waymark A { get; set; }
        public Waymark B { get; set; }
        public Waymark C { get; set; }
        public Waymark D { get; set; }
        public Waymark One { get; set; }
        public Waymark Two { get; set; }
        public Waymark Three { get; set; }
        public Waymark Four { get; set; }

        /// <summary>
        /// Property Changed event handler for this model.
        /// </summary>
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        /// <summary>
        /// based on PunishedPineapple's WaymarkPresetPlugin: https://github.com/PunishedPineapple/WaymarkPresetPlugin/blob/master/WaymarkPresetPlugin/WaymarkPreset.cs
        /// </summary>
        public Preset Parse(byte[] rawData) {
            if (rawData.Length != 104) {
                throw new Exception("Unexpected data length in WaymarkPreset.Parse");
            }
            //Preset preset = new Preset();

            var p = this;
            this.A=new Waymark();
            this.A.X = BitConverter.ToInt32(rawData, 0) / 1000.0f;
            this.A.Y = BitConverter.ToInt32(rawData, 4) / 1000.0f;
            this.A.Z = BitConverter.ToInt32(rawData, 8) / 1000.0f;
            this.A.Active = (rawData[96] & 0b00000001) > 0;
            this.A.ID = (WaymarkID)0;

            this.B = new Waymark();
            this.B.X = BitConverter.ToInt32(rawData, 12) / 1000.0f;
            this.B.Y = BitConverter.ToInt32(rawData, 16) / 1000.0f;
            this.B.Z = BitConverter.ToInt32(rawData, 20) / 1000.0f;
            this.B.Active = (rawData[96] & 0b00000010) > 0;
            this.B.ID = (WaymarkID)1;

            this.C = new Waymark();
            this.C.X = BitConverter.ToInt32(rawData, 24) / 1000.0f;
            this.C.Y = BitConverter.ToInt32(rawData, 28) / 1000.0f;
            this.C.Z = BitConverter.ToInt32(rawData, 32) / 1000.0f;
            this.C.Active = (rawData[96] & 0b00000100) > 0;
            this.C.ID = (WaymarkID)2;

            this.D = new Waymark();
            this.D.X = BitConverter.ToInt32(rawData, 36) / 1000.0f;
            this.D.Y = BitConverter.ToInt32(rawData, 40) / 1000.0f;
            this.D.Z = BitConverter.ToInt32(rawData, 44) / 1000.0f;
            this.D.Active = (rawData[96] & 0b00001000) > 0;
            this.D.ID = (WaymarkID)3;

            this.One = new Waymark();
            this.One.X = BitConverter.ToInt32(rawData, 48) / 1000.0f;
            this.One.Y = BitConverter.ToInt32(rawData, 52) / 1000.0f;
            this.One.Z = BitConverter.ToInt32(rawData, 56) / 1000.0f;
            this.One.Active = (rawData[96] & 0b00010000) > 0;
            this.One.ID = (WaymarkID)4;

            this.Two = new Waymark();
            this.Two.X = BitConverter.ToInt32(rawData, 60) / 1000.0f;
            this.Two.Y = BitConverter.ToInt32(rawData, 64) / 1000.0f;
            this.Two.Z = BitConverter.ToInt32(rawData, 68) / 1000.0f;
            this.Two.Active = (rawData[96] & 0b00100000) > 0;
            this.Two.ID = (WaymarkID)5;

            this.Three = new Waymark();
            this.Three.X = BitConverter.ToInt32(rawData, 72) / 1000.0f;
            this.Three.Y = BitConverter.ToInt32(rawData, 76) / 1000.0f;
            this.Three.Z = BitConverter.ToInt32(rawData, 80) / 1000.0f;
            this.Three.Active = (rawData[96] & 0b01000000) > 0;
            this.Three.ID = (WaymarkID)6;

            this.Four = new Waymark();
            this.Four.X = BitConverter.ToInt32(rawData, 84) / 1000.0f;
            this.Four.Y = BitConverter.ToInt32(rawData, 88) / 1000.0f;
            this.Four.Z = BitConverter.ToInt32(rawData, 92) / 1000.0f;
            this.Four.Active = (rawData[96] & 0b10000000) > 0;
            this.Four.ID = (WaymarkID)7;

            this.MapID = BitConverter.ToUInt16(rawData, 98);
            //preset.Time = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt32(rawData, 100));
            return p;
        }

        public byte[] ConstructGamePreset() {
            //	List is easy because we can just push data on to it.
            List<byte> byteData = new List<byte>();

            //	Waymark coordinates.
            if (A == null)
                A = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(A.Active ? (Int32)(A.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(A.Active ? (Int32)(A.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(A.Active ? (Int32)(A.Z * 1000.0) : 0));

            if (B == null)
                B = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(B.Active ? (Int32)(B.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(B.Active ? (Int32)(B.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(B.Active ? (Int32)(B.Z * 1000.0) : 0));

            if (C == null)
                C = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(C.Active ? (Int32)(C.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(C.Active ? (Int32)(C.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(C.Active ? (Int32)(C.Z * 1000.0) : 0));

            if (D == null)
                D = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(D.Active ? (Int32)(D.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(D.Active ? (Int32)(D.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(D.Active ? (Int32)(D.Z * 1000.0) : 0));

            if (One == null)
                One = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(One.Active ? (Int32)(One.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(One.Active ? (Int32)(One.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(One.Active ? (Int32)(One.Z * 1000.0) : 0));

            if (Two == null)
                Two = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(Two.Active ? (Int32)(Two.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(Two.Active ? (Int32)(Two.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(Two.Active ? (Int32)(Two.Z * 1000.0) : 0));

            if (Three == null)
                Three = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(Three.Active ? (Int32)(Three.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(Three.Active ? (Int32)(Three.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(Three.Active ? (Int32)(Three.Z * 1000.0) : 0));

            if (Four == null)
                Four = new Waymark();
            byteData.AddRange(BitConverter.GetBytes(Four.Active ? (Int32)(Four.X * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(Four.Active ? (Int32)(Four.Y * 1000.0) : 0));
            byteData.AddRange(BitConverter.GetBytes(Four.Active ? (Int32)(Four.Z * 1000.0) : 0));

            //	Which waymarks are active.
            byte activeMask = 0x00;
            if (A.Active) activeMask |= 0b00000001;
            if (B.Active) activeMask |= 0b00000010;
            if (C.Active) activeMask |= 0b00000100;
            if (D.Active) activeMask |= 0b00001000;
            if (One.Active) activeMask |= 0b00010000;
            if (Two.Active) activeMask |= 0b00100000;
            if (Three.Active) activeMask |= 0b01000000;
            if (Four.Active) activeMask |= 0b10000000;
            byteData.Add(activeMask);

            //	Reserved byte.
            byteData.Add((byte)0x00);

            //	Territory ID.
            byteData.AddRange(BitConverter.GetBytes(MapID));

            //	Time last modified.
            DateTimeOffset Time = new DateTimeOffset(DateTimeOffset.Now.UtcDateTime);
            byteData.AddRange(BitConverter.GetBytes((Int32)Time.ToUnixTimeSeconds()));

            //	Shouldn't ever come up with the wrong length, but just in case...
            if (byteData.Count != 104) {
                throw new Exception("Error in WaymarkPreset.ConstructGamePreset(): Constructed byte array was of an unexpected length.");
            }

            //	Send it out.
            return byteData.ToArray();
        }

        public Preset Normalized() {
            var p = this;
            if (p.A == null)
                p.A = new Waymark();
            if (p.B == null)
                p.B = new Waymark();
            if (p.C == null)
                p.C = new Waymark();
            if (p.D == null)
                p.D = new Waymark();
            if (p.One == null)
                p.Two = new Waymark();
            if (p.Three == null)
                p.Three = new Waymark();
            if (p.Four == null)
                p.Four = new Waymark();
            return p;
        }

        //public string GetPresetDataString() {
        //	//	Try to get the zone name from the function passed to us if we can.
        //	//	Construct the string.
        //	string str = "";
        //	str += "A: " + A.GetWaymarkDataString() + "\r\n";
        //	str += "B: " + B.GetWaymarkDataString() + "\r\n";
        //	str += "C: " + C.GetWaymarkDataString() + "\r\n";
        //	str += "D: " + D.GetWaymarkDataString() + "\r\n";
        //	str += "1: " + One.GetWaymarkDataString() + "\r\n";
        //	str += "2: " + Two.GetWaymarkDataString() + "\r\n";
        //	str += "3: " + Three.GetWaymarkDataString() + "\r\n";
        //	str += "4: " + Four.GetWaymarkDataString() + "\r\n";
        //	str += "Zone: " + Name + "\r\n";
        //	//str += "Last Modified: " + Time.LocalDateTime.ToString();
        //	return str;
        //}
    }
}
