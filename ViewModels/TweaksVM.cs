using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRep_Code.Utilities;

namespace WinRep_Code.ViewModels
{
    public class TweaksVM : BaseViewModel
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

        public RelayCommand NavigateToCustomTweaksCommand { get; set; }

        public TweaksVM(INavigationService navigation)
        {
            NavService = navigation;
            NavigateToCustomTweaksCommand = new RelayCommand(_ => NavService.NavigateTo<CustomTweaksVM>(), _ => true);
        }
    }
}
