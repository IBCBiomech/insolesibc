using CommunityToolkit.Mvvm.ComponentModel;
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
    [ObservableObject]
    public partial class InsoleInfo
    {
        private static HashSet<int> idsUsed = new();
        private static Dictionary<Side, InsoleInfo> sidesUsed = new();
        [ObservableProperty]
        private bool isSelected;
        [ObservableProperty]
        private int? id;
        public byte? handler { get; set; }
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private Side? side;
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
        [ObservableProperty]
        private string address;
        [ObservableProperty]
        private int? battery;
        [ObservableProperty]
        private bool connected;
        [ObservableProperty]
        private string? fw;
        public InsoleInfo(string name, string address)
        {
            Id = null;
            Name = name;
            Side = null;
            Address = address;
            Battery = null;
            Connected = false;
            Fw = null;
            handler = null;
        }
        public InsoleInfo(InsoleScanData insole)
        {
            Id = getNextID();
            Name = insole.name;
            Side = null;
            Address = insole.MAC;
            Battery = null;
            Connected = false;
            Fw = null;
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
