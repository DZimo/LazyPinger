using CommunityToolkit.Maui;
using LazyPinger.Base.IServices;
using LazyPingerMAUI.ViewModels;

namespace LazyPingerMAUI.Views
{
    public partial class SettingsPage : ContentPage
    {
        private INetworkService _networkService { get; set; }

        private MainViewModel _mainViewModel { get; set; }

        public SettingsPage(INetworkService networkService, MainViewModel mainViewModel, IPopupService popupService)
        {
            InitializeComponent();

            _networkService = networkService;
            _mainViewModel = mainViewModel;
            BindingContext = new SettingsViewModel(networkService, mainViewModel, popupService);
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
