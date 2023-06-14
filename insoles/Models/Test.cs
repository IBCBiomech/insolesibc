using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class Test
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Nombre { get; set; }
        public string csv { get; set; }
        public string? video1 { get; set; }
        public string? video2 { get; set; }
        public Test(DateTime date, string csv)
        {
            this.Nombre = "Test";
            this.Date = date;
            this.csv = csv;
        }
        public Test(DateTime date, string csv, List<string> videos) : this(date, csv)
        {
            if(videos.Count >= 1)
            {
                this.video1 = videos[0];
            }
            if(videos.Count >= 2)
            {
                this.video2 = videos[1];
            }
        }
    }
}
