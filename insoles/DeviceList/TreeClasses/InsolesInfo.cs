using insoles.Common;
using insoles.DeviceList.Enums;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;

namespace insoles.DeviceList.TreeClasses
{
    // Guarda la información de una Insole
    public class InsolesInfo : BaseObject
    {
        private static HashSet<int> idsUsed = new HashSet<int>();
        private static Dictionary<Side, InsolesInfo> sidesUsed = new Dictionary<Side, InsolesInfo>();
        public int? id
        {
            get { return GetValue<int>("id"); }
            set { SetValue("id", value); }
        }
        public byte? handler { get; set; }
        public string name
        {
            get { return GetValue<string>("name"); }
            set { SetValue("name", value); }
        }
        public Side? side
        {
            get { return GetValue<Side?>("side"); }
            set 
            {
                if(side != null) // Libera la que estaba usando
                {
                    sidesUsed.Remove(side.Value);
                }
                if(value != null)
                {
                    if (sidesUsed.ContainsKey(value.Value)) // Estaba usado ese side?
                    {
                        InsolesInfo insoleReplaced = sidesUsed[value.Value]; // Insole que usaba ese side
                        insoleReplaced.replaceSide();
                    }
                    sidesUsed[value.Value] = this;
                    SetValue("side", value);
                }
            }
        }
        public void replaceSide()
        {
            Side? oldSide = this.side;
            Side? unusedSide = getUnusedSide();
            sidesUsed.Remove(oldSide.Value);
            side = unusedSide;
        }
        private static Side? getUnusedSide()
        {
            foreach (Side side in Enum.GetValues(typeof(Side)))
            {
                if (!sidesUsed.ContainsKey(side))
                {
                    return side;
                }
            }
            return null;
        }
        public string address
        {
            get { return GetValue<string>("adress"); }
            set { SetValue("adress", value); }
        }
        public int? battery
        {
            get { return GetValue<int?>("battery"); }
            set { SetValue("battery", value); }
        }
        public bool connected
        {
            get { return GetValue<bool>("connected"); }
            set { SetValue("connected", value); }
        }
        public string? fw
        {
            get { return GetValue<string>("fw"); }
            set { SetValue("fw", value); }
        }
        public InsolesInfo(string name, string address)
        {
            this.id = null;
            this.name = name;
            this.side = null;
            this.address = address;
            this.battery = null;
            this.connected = false;
            this.fw = null;
            this.handler = null;
        }
        public void setID()
        {
            id = getNextID();
        }
        private static int getNextID()
        {
            for (int i = 0; i < idsUsed.Count; i++)
            {
                if (!idsUsed.Contains(i))
                {
                    idsUsed.Add(i);
                    return i;
                }
            }
            int id = idsUsed.Count;
            idsUsed.Add(id);
            return id;
        }
    }
}
