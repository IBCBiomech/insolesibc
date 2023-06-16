using insoles.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace insoles.Forms
{
    /// <summary>
    /// Lógica de interacción para TextInputForm.xaml
    /// </summary>
    public partial class TextInputForm : Window, INotifyPropertyChanged
    {
        private string _text = "";
        public string text { 
            get
            { 
                return _text; 
            }
            set 
            {
                _text = value;
                OnPropertyChanged();
            } }
        public delegate void TextEventHandler(object sender, string text);
        public event TextEventHandler enterEvent;
        public event TextEventHandler escEvent;
        public event PropertyChangedEventHandler? PropertyChanged;

        public TextInputForm()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                enterEvent?.Invoke(sender, text);
                Close();
            }
            if(e.Key == Key.Escape)
            {
                escEvent?.Invoke(sender, text);
                Close();
            }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
