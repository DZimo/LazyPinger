using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Core.ViewModels;

namespace LazyPingerMAUI.ViewModels
{
    public partial class NetworkViewModel : ViewModelBase
    {
        public MainViewModel MainVm { get; set; }

        [ObservableProperty]
        private VmNetworkUser vmNetworkUser = new();

        public NetworkViewModel(INetworkService networkService, MainViewModel mainViewModel)
        {
            MainVm = mainViewModel;
        }

        [RelayCommand]
        public async Task StartTcpServer()
        {
           await MainVm.NetworkService.StartTcpServer(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.TcpPort);
        }

        [RelayCommand]
        public async Task StartUdpServer()
        {
            await MainVm.NetworkService.StartUdpServer(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.UdpPort);
        }
    }
}
