using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.Network;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace LazyPinger.Base.IServices
{
    public interface INetworkService
    {   
        public NetworkSettings NetworkSettings { get; set; }

        public Task InitNetworkSettings();

        public Task<TcpListener?> StartTcpServer(string? selectedIP, int selectedPort);

        public Task<UdpClient?> StartUdpServer(string? selectedIP, int selectedPort);

        public Task<TcpClient?> StartTcpClient(string? selectedIP, int selectedPort);

        public Task<UdpClient?> StartUdpClient(string? selectedIP, int selectedPort);

        public Task<bool> PingAll( ObservableCollection<DevicePing> foundDevices);

        public Task<bool> PingAllAsync(ObservableCollection<DevicePing> foundDevices);

        public List<string> GetMacAddresses();

        public Task<bool> SendTCP(string? selectedIP, string messageToSend, int defaultPort, bool broadcast = false);

        public Task<bool> SendUDP(string? selectedIP, string messageToSend, int defaultPort, bool broadcast = false);
    }
}
