using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class InformeFile : ModelBase
    {
        public int Id { get; set; }
        public int InformeId { get; set; }
        public Informe Informe { get; set; }
        public DateTime Date { get; set; }
        public string path { get; set; }
        public InformeFile(string path, DateTime Date) 
        {
            this.path = path;
            this.Date = Date;
        }
    }
}
