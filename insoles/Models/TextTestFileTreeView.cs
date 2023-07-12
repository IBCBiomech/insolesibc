using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Models
{
    public class TextTestFileTreeView : TestFileTreeView
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public ICommand RenombrarCommand { get; set; }
        public TextTestFileTreeView(string name, DateTime date, ICommand RenombrarCommand) 
        { 
            this.Name = name;
            this.Date = date;
            this.RenombrarCommand = RenombrarCommand;
        }
    }
}
