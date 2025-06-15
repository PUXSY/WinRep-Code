using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRep_Code.ViewModels;

namespace WinRep_Code.Utilities
{
    public interface INavigationService
    {
        BaseViewModel CurrentViewModel { get; }
        void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
    }

    internal class NavigationService : BaseViewModel, INavigationService
    {
        private BaseViewModel _currentViewModel;
        private Func<Type, BaseViewModel> _viewModelFactory;

        public BaseViewModel CurrentViewModel {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public NavigationService(Func<Type , BaseViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            BaseViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
            CurrentViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel), "ViewModel cannot be null.");
        }
    }
}
