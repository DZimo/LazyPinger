using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Core.ViewModels;
using System.Net.Sockets;
using System.Text;

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
            //if (!VmNetworkUser.TcpListener.Server.Connected)
            //    return;

            var res = await MainVm.NetworkService.StartTcpServer(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.TcpPort);

            if (res is null)
                return;

           //if (!res.Server.Connected)
           //     return;

           VmNetworkUser.IsTcpConnected = true;
           VmNetworkUser.TcpStatusColor = "#00FF00";
           VmNetworkUser.TcpListener = res;
           _ = TcpReceiver();
        }

        [RelayCommand]
        public async Task StartUdpServer()
        {
            var res = await MainVm.NetworkService.StartUdpServer(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.UdpPort);
            VmNetworkUser.UdpClient = res;
            _ = UdpReceiver();
        }

        [RelayCommand]
        public async Task SendTcpMessage()
        {
           var res = await MainVm.NetworkService.SendTCP(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.SentTcpMessage, VmNetworkUser.TcpPort);
        }

        [RelayCommand]
        public async Task SendUdpMessage()
        {
            await MainVm.NetworkService.StartUdpServer(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.UdpPort);
        }

        public async Task TcpReceiver()
        {
            while (true)
            {
                if (VmNetworkUser.TcpListener is null)
                {
                    await Task.Delay(1);
                    continue;
                }

                var client = await VmNetworkUser.TcpListener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                var buffer = new byte[client.ReceiveBufferSize];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                VmNetworkUser.ReceivedTcpMessage = response;
                await Task.Delay(1000);
            }
        }

        public async Task UdpReceiver()
        {

        }
    }
}
