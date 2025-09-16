using LazyPinger.Base.IServices;
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

            this.BindingContext = new NetworkViewModel(networkService, mainVm);
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
