using mvvm.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisewalkSDK.Protocol_v3;

namespace mvvm.Models
{
    public class InsoleInfo
    {
        private static HashSet<int> idsUsed = new();
        private static Dictionary<Side, InsoleInfo> sidesUsed = new();
        public int? id { get; set; }
        public byte? handler { get; set; }
        public string name { get; set; }
        private Side? _side;
        public Side? side
        {
            get { return _side; }
            set
            {
                _side = value;
                return;
                if (side != null) // Libera la que estaba usando
                {
                    sidesUsed.Remove(side.Value);
                }
                if (value != null)
                {
                    if (sidesUsed.ContainsKey(value.Value)) // Estaba usado ese side?
                    {
                        InsoleInfo insoleReplaced = sidesUsed[value.Value]; // Insole que usaba ese side
                        insoleReplaced.replaceSide();
                    }
                    sidesUsed[value.Value] = this;
                    side = value;
                }
            }
        }
        public void replaceSide()
        {
            Side? oldSide = this.side;
            Side? unusedSide = getUnusedSide();
            if(oldSide != null)
                sidesUsed.Remove(oldSide.Value);
            side = unusedSide;
        }
        private static Side? getUnusedSide()
        {
            Trace.WriteLine("getUnusedSide");
            foreach (Side side in Enum.GetValues(typeof(Side)))
            {
                if (!sidesUsed.ContainsKey(side))
                {
                    return side;
                }
            }
            return null;
        }
        public string address { get; set; }
        public int? battery { get; set; }
        public bool connected { get;set; }
        public string? fw { get; set; }
        public InsoleInfo(string name, string address)
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
        public InsoleInfo(InsoleScanData insole)
        {
            id = getNextID();
            name = insole.name;
            side = null;
            address = insole.MAC;
            connected = false;
            fw = null;
            handler = null;
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
