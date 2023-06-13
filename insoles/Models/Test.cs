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
        public string csv { get; set; }
        public string? video1 { get; set; }
        public string? video2 { get; set; }
        public Test(DateTime date, string csv)
        {
            this.Date = date;
            this.csv = csv;
        }
    }
}
