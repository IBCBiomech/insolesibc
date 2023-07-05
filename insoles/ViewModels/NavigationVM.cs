using insoles.Commands;
using insoles.Utilities;
using System.Windows;
using System.Windows.Input;

namespace insoles.ViewModel
{
    public class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand RegistroCommand { get; set; }
        public ICommand AnalisisCommand { get; set; }
        public ICommand InformesCommand { get; set; }
        public ResetLayoutCommand ResetLayoutCommand { get; set; }


        private void Home(object obj){
            CurrentView = new HomeVM();
        }
        private void Registro(object obj)
        {
            CurrentView = new RegistroVM();
        }
        private void Analisis(object obj)
        {
            CurrentView = new AnalisisVM();
        }
        private void Informes(object obj)
        {
            CurrentView = new InformesVM();
        }
        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            RegistroCommand = new RelayCommand(Registro);
            AnalisisCommand = new RelayCommand(Analisis);
            InformesCommand = new RelayCommand(Informes);
            ResetLayoutCommand = new ResetLayoutCommand();

            // Startup Page
            CurrentView = new HomeVM();
        }
    }
}
