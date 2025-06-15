using System;
using WinRep_Code.Utilities;

namespace WinRep_Code.ViewModels
{
    public class CustomTweaksVM : BaseViewModel
    {
        private readonly INavigationService _navService;

        public INavigationService NavService => _navService;

        public RelayCommand NavigateToTweaksCommand { get; }

        public CustomTweaksVM(INavigationService navigation)
        {
            _navService = navigation ?? throw new ArgumentNullException(nameof(navigation));
            NavigateToTweaksCommand = new RelayCommand(_ => NavService.NavigateTo<TweaksVM>(), _ => true);
        }
    }
}