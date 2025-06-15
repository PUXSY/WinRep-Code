using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRep_Code.Utilities;

namespace WinRep_Code.ViewModels
{
    public class ScanVM : BaseViewModel
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

        public RelayCommand NavigateToScanSettingsCommand { get; set; }

        public ScanVM (INavigationService navigation)
        {
            NavService = navigation;
            NavigateToScanSettingsCommand = new RelayCommand(_ => NavService.NavigateTo<ScanSettingsVM>(), _ => true);
        }
    }
}
