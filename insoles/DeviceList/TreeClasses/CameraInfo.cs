using insoles.Common;
using insoles.DeviceList.Enums;
using System;
using System.Collections.Generic;
using static WisewalkSDK.Protocol_v3;

namespace insoles.DeviceList.TreeClasses
{
    // Guarda la información de una camara
    public class CameraInfo : BaseObject
    {
        private static Dictionary<Position, CameraInfo> positionsUsed = new Dictionary<Position, CameraInfo>();
        public static EventHandler positionChanged;
        public string name
        {
            get { return GetValue<string>("name"); }
            set { SetValue("name", value); }
        }
        public Position? position
        {
            get { return GetValue<Position?>("position"); }
            set
            {
                if (position != null) // Libera la que estaba usando
                {
                    positionsUsed.Remove(position.Value);
                }
                if (value != null)
                {
                    if (positionsUsed.ContainsKey(value.Value)) // Estaba usado ese side?
                    {
                        CameraInfo cameraReplaced = positionsUsed[value.Value]; // Insole que usaba ese side
                        cameraReplaced.replacePosition();
                    }
                    positionsUsed[value.Value] = this;
                    SetValue("position", value);
                }
                positionChanged?.Invoke(this, new EventArgs());
            }
        }
        public void replacePosition()
        {
            Position? oldPosition = this.position;
            Position? unusedPosition = getUnusedPosition();
            positionsUsed.Remove(oldPosition.Value);
            position = unusedPosition;
        }
        private static Position? getUnusedPosition()
        {
            foreach (Position position in Enum.GetValues(typeof(Position)))
            {
                if (!positionsUsed.ContainsKey(position))
                {
                    return position;
                }
            }
            return null;
        }
        public int number
        {
            get { return GetValue<int>("number"); }
            set { SetValue("number", value); }
        }
        public int? fps
        {
            get { return GetValue<int?>("fps"); }
            set { SetValue("fps", value); }
        }
        public CameraInfo(int number, string name)
        {
            this.number = number;
            this.name = name;
            this.fps = null;
            this.position = null;
        }
    }
}
