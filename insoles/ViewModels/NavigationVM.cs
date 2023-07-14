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

        private HomeVM homeVM;
        private RegistroVM registroVM;
        private AnalisisVM analisisVM;
        private InformesVM informesVM;
        private void Home(object obj){
            if(homeVM == null)
            {
                homeVM = new HomeVM();
            }
            CurrentView = homeVM;
        }
        private void Registro(object obj)
        {
            if (registroVM == null)
            {
                registroVM = new RegistroVM();
            }
            CurrentView = registroVM;
        }
        private void Analisis(object obj)
        {
            if (analisisVM == null)
            {
                analisisVM = new AnalisisVM();
            }
            CurrentView = analisisVM;
        }
        private void Informes(object obj)
        {
            if (informesVM == null)
            {
                informesVM = new InformesVM();
            }
            CurrentView = informesVM;
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
