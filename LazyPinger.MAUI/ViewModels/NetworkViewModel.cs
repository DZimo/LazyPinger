using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Core.ViewModels;
using System.Net;
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
        public void StopTcpServer()
        {
            if (VmNetworkUser.TcpListener is null)
                return;

            VmNetworkUser.TcpListener.Stop();
            VmNetworkUser.IsTcpConnected = false;
            VmNetworkUser.TcpStatusColor = "#FF0000";
        }

        [RelayCommand]
        public async Task StartUdpServer()
        {
            var res = await MainVm.NetworkService.StartUdpServer(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.UdpPort);

            VmNetworkUser.IsUdpConnected = true;
            VmNetworkUser.UdpStatusColor = "#00FF00";
            VmNetworkUser.UdpClient = res;

            _ = UdpReceiver();
        }


        [RelayCommand]
        public void StopUdpServer()
        {
            if (VmNetworkUser.UdpClient is null)
                return;

            VmNetworkUser.UdpClient.Close();
            VmNetworkUser.IsUdpConnected = false;
            VmNetworkUser.UdpStatusColor = "#FF0000";
        }


        [RelayCommand]
        public async Task SendTcpMessage()
        {
           var res = await MainVm.NetworkService.SendTCP(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.SentTcpMessage, VmNetworkUser.TcpPort);
        }

        [RelayCommand]
        public async Task SendUdpMessage()
        {
            await MainVm.NetworkService.SendUDP(MainVm.NetworkService.NetworkSettings.IpAddress, VmNetworkUser.SentUdpMessage, VmNetworkUser.UdpPort);
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
            while (true)
            {
                if (VmNetworkUser.UdpClient is null)
                {
                    await Task.Delay(1);
                    continue;
                }

                var anyIP = new IPEndPoint(IPAddress.Any, 0);
                var clientRes = await VmNetworkUser.UdpClient.ReceiveAsync();
                var data = Encoding.ASCII.GetString(clientRes.Buffer);

                VmNetworkUser.ReceivedUdpMessage = data;
                await Task.Delay(1000);
            }
        }
    }
}
