using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models;
using LazyPinger.Core.Utils;
using LazyPinger.Core.ViewModels;
using LazyPingerMAUI.ViewModels;

namespace LazyPingerMAUI.Views
{
    public partial class NetworkPage : ContentPage
    {
        private INetworkService _networkService { get; set; }

        private MainViewModel _mainViewModel { get; set; }

        public NetworkPage(INetworkService networkService, MainViewModel mainVm)
        {
            InitializeComponent();

            _networkService = networkService;
            _mainViewModel = mainVm;

            this.BindingContext = _mainViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }

}
