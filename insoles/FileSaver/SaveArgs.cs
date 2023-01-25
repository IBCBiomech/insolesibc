using System;

namespace insoles.FileSaver
{
    public class SaveArgs: EventArgs
    {
        public string directory { get; set; }
        public bool csv { get; set; }
        public bool video { get; set; }
    }
}
