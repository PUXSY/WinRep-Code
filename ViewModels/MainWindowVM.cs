using System;
using WinRep_Code.Utilities;

namespace WinRep_Code.ViewModels
{
    public class MainWindowVM : BaseViewModel
    {
        private INavigationService _navService;
        public INavigationService NavService
        {
            get => _navService;
            set
            {
                if (_navService != value)
                {
                    _navService = value;
                    OnPropertyChanged();
                }
            }
        }
        public RelayCommand NavigateToScanCommand { get; set; }
        public RelayCommand NavigateToTweaksCommand { get; set; }
        public RelayCommand NavigateToInstallCommand { get; set; }

        public MainWindowVM(INavigationService navigation = null)
        {
            NavService = navigation;
            NavigateToScanCommand = new RelayCommand(_ => NavService.NavigateTo<ScanVM>(), _ => true);
            NavigateToTweaksCommand = new RelayCommand(_ => NavService.NavigateTo<TweaksVM>(), _ => true);
            NavigateToInstallCommand = new RelayCommand(_ => NavService.NavigateTo<InstallVM>(), _ => true);
        }
    }
}