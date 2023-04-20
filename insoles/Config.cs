using insoles.Graphs;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using ScottPlot.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;

using Quality = insoles.Graphs.Foot.Quality;

namespace insoles
{
    public static class Config
    {
        public static string[] allowedExtensions = new string[] { ".txt", ".csv", ".c3d", ".avi", ".mov", ".mp4" };
        public const bool showOnlyInitialPath = true;
        public const int NUMPACKETS = 4;
        public static string INITIAL_PATH
        {
            get
            {
                string userName = Environment.UserName;
                return "C:\\Users\\" + userName + "\\Documents";
            }
        }
        public const int VIDEO_FPS = 30;
        public const int VIDEO_FPS_SAVE = 25;
        public const int FRAME_HEIGHT = 480;
        public const int FRAME_WIDTH = 640;
        public const int RENDER_MS_CAPTUE = 20;
        public static readonly MatType DEFAULT_MAT_TYPE = MatType.CV_8UC3; //Se tienen que grabar todos los frames con el mismo tipo de datos
        public static MatType? MAT_TYPE = null;
        public const string csvHeaderInsoles = @"DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT	DEFAULT
            TIME	TIME	LARCH	LHALLUX	LHEELR	LHEELL	LMET1	LMET3	LMET5	LTOES	RARCH	RHALLUX	RHEELR	RHEELL	RMET1	RMET3	RMET5	RTOES	LTOTAL	RTOTAL
            FRAME_NUMBERS	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG	ANALOG
            ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL	ORIGINAL
        ITEM	0	0	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x	x"
        + "\n";
        public static Color colorX = Color.Red;
        public static Color colorY = Color.Green;
        public static Color colorZ = Color.Blue;
        public static Color colorW = Color.Orange;
        public const float BACKGROUND = -1;
        public const Quality footQuality = Quality.MID;
        public static Colormap colormap = Colormap.Jet;
        public static Dictionary<Quality, double> qualitySizes = new Dictionary<Quality, double>() 
        {
            [Quality.HIGH] = 1,
            [Quality.MID] = 0.5,
            [Quality.LOW] = 0.25
        };
        public const int NUM_SENSORS = 8;
    }
}
