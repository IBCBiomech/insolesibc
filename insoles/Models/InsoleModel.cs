using insoles.Enums;
using insoles.Utilities;
using insoles.ViewModel;
using System;
using System.Collections.Generic;

namespace insoles.Model
{
    public class InsoleModel : ModelBase
    {
        private static HashSet<int> idsUsed = new HashSet<int>();
        private static Dictionary<Side, InsoleModel> sidesUsed = new Dictionary<Side, InsoleModel>();
        public RegistroVM VM { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string MAC { get; set; }
        private int? _battery;
        public int? battery { get { return _battery; } set { _battery = value; OnPropertyChanged(); } }
        private string _fw;
        public string fw { get { return _fw; } set { _fw = value; OnPropertyChanged(); } }
        private bool _connected;
        public bool connected { get { return _connected; } set { _connected = value; OnPropertyChanged(); }}
        private Side? _side;
        public Side? side
        {
            get { return _side; }
            set
            {
                if (side != null) // Libera la que estaba usando
                {
                    sidesUsed.Remove(side.Value);
                }
                if (value != null)
                {
                    if (sidesUsed.ContainsKey(value.Value)) // Estaba usado ese side?
                    {
                        InsoleModel insoleReplaced = sidesUsed[value.Value]; // Insole que usaba ese side
                        insoleReplaced.replaceSide();
                    }
                    sidesUsed[value.Value] = this;
                    _side = value; 
                    OnPropertyChanged();
                }
            }
        }
        public void replaceSide()
        {
            Side? oldSide = side;
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
        public InsoleModel(string name, string MAC, RegistroVM VM)
        {
            this.id = getNextID();
            this.name = name;
            this.MAC = MAC;
            this.VM = VM;
            this.connected = false;
            this.battery = null;
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
