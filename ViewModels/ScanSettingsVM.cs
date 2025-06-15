using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRep_Code.Utilities;

namespace WinRep_Code.ViewModels
{
    public class ScanSettingsVM : BaseViewModel
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

        public ScanSettingsVM(INavigationService navigation)
        {
            NavService = navigation;
            NavigateToScanCommand = new RelayCommand(_ => NavService.NavigateTo<ScanVM>(), _ => true);
        }
    }
}
