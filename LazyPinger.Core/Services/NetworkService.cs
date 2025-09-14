using LazyPinger.Base.IServices;
using LazyPinger.Base.Localization;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.Network;
using LazyPinger.Core.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace LazyPinger.Core.Services
{
    public class NetworkService : INetworkService
    {
        private List<Task> pingTaskList = new List<Task>();
        public NetworkSettings NetworkSettings { get; set; } = new();

        private ITextParserService textParserService;

        public NetworkService(ITextParserService textParserService)
        {
            this.textParserService = textParserService;
        }

        public async Task InitNetworkSettings()
        {
            var res = await GetHostIPs();
            NetworkSettings.HostAddresses = res.ToList();
            var ip = res.FirstOrDefault();

            if (ip is null)
                return;

            NetworkSettings.IpAddress = ip.ToString();
        }

        public async Task<IPAddress[]> GetHostIPs()
        {
            var ipList = new List<IPAddress>();

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            networkInterfaces.ToList().ForEach(o =>
            {
                var ipProperties = o.GetIPProperties();
                ipProperties.UnicastAddresses.ToList().ForEach(p =>
                {
                    if (p.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(p.Address))
                        return;

                    ipList.Add(p.Address);
                });

            });
            return ipList.ToArray();
        }

        public Task<TcpListener?> StartTcpServer(string? selectedIP, int selectedPort)
        {
            if (selectedIP is null)
                return null;

            var serverIP = IPAddress.Parse(selectedIP);
            var server = new TcpListener(serverIP, selectedPort);
            server.Start();
            return Task.FromResult<TcpListener?>(server);
        }

        public async Task<UdpClient?> StartUdpServer(string? selectedIP, int selectedPort)
        {
            if (selectedIP is null)
                return null;

            var server = new UdpClient(selectedPort);
            //server.Connect(selectedIP, selectedPort);
            return server;
        }

        public async Task<TcpClient?> StartTcpClient(string? selectedIP, int selectedPort)
        {
            if (selectedIP is null)
                return null;

            var longIP = textParserService.AddressToLong(selectedIP);
            var ipEndPoint = new IPEndPoint(longIP, selectedPort);
            var client = new TcpClient();
            await client.ConnectAsync(ipEndPoint);
            return client;
        }
        public async Task<UdpClient?> StartUdpClient(string? selectedIP, int selectedPort)
        {
            if (selectedIP is null)
                return null;

            var serverIP = IPAddress.Parse(selectedIP);
            var ipEndPoint = new IPEndPoint(serverIP, selectedPort);

            var server = new UdpClient();
            server.Connect(ipEndPoint);
            return server;
        }

        public async Task<bool> PingAll(ObservableCollection<DevicePing> foundDevices)
        {
            await PingAllAsync(foundDevices);
            return true;
        }

        public List<string> GetMacAddresses()
        {
            var macs = new List<string>();
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                macs.Add(adapter.GetPhysicalAddress().ToString());

                var adapterProperties = adapter.GetIPProperties();

                var result = adapterProperties.UnicastAddresses
                    .Select(o => o.Address.ToString())
                    .ToList();

                result.ForEach(Console.WriteLine);
            }

            return macs;
        }

        public async Task<bool> SendTCP(string? selectedIP, string msg, int defaultPort, bool broadcast = false)
        {
            try
            {
                var client = await StartTcpClient(selectedIP, defaultPort);
                await using NetworkStream stream = client.GetStream();

                byte[] messageBytes = Encoding.UTF8.GetBytes(msg);
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public async Task<bool> SendUDP(string? selectedIP, string msg, int defaultPort, bool broadcast = false)
        {
            try
            {
                var client = await StartUdpClient(selectedIP, defaultPort);
                byte[] messageBytes = Encoding.UTF8.GetBytes(msg);

                var res = await client.SendAsync(messageBytes, messageBytes.Length);

                //var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //var addressToSend = IPAddress.Parse(selectedIP);
                //var endPoint = new IPEndPoint(addressToSend, defaultPort);
                //udpSocket.EnableBroadcast = broadcast;

                //byte[] msgBuffer = Encoding.ASCII.GetBytes(msg);

                //udpSocket.SendTo(msgBuffer, endPoint);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> PingAllAsync(ObservableCollection<DevicePing> foundDevices)
        {
            PingTaskPoolCreator(128, ref foundDevices);
            await Task.WhenAll(pingTaskList);
            return true;
        }

        private void PingTaskPoolCreator(int numTasks, ref ObservableCollection<DevicePing> foundDevices)
        {
            var it = 1;

            while (it < numTasks)
            {
                pingTaskList.Add(PingIP(NetworkSettings.MaxSubnetRange / numTasks * (it - 1), NetworkSettings.MaxSubnetRange / numTasks * it, foundDevices));
                it++;
            }
        }

        private async Task PingIP(int fromIP, int toIP, ObservableCollection<DevicePing> foundDevices)
        {
            var ping = new Ping();

            for (int i = fromIP; i < toIP; i++)
            {
                if (i == 255)
                    continue;

                var ipAddressToPing = NetworkSettings.SubnetAddress + i;
                var sendPing = await ping.SendPingAsync(ipAddressToPing, NetworkSettings.PingTimeout);

                if (sendPing.Status != IPStatus.Success)
                    return;

                var foundIP = sendPing.Address.ToString();
                //var arpDetector = new(ipAddressToPing)
                //await ArpDetectorService.ArpInit();

                var devicesPings = ListenVm.Instance.DevicesPingVm;

                if (foundDevices.ToList().Exists(o => o.IP == foundIP))
                    continue;

                var found = devicesPings?.Where(o => o.Entity.IP == foundIP || (textParserService.AddressToIntWithSubnet(o.MinRange) <= textParserService.AddressToIntWithSubnet(foundIP)
                                                                               && textParserService.AddressToIntWithSubnet(o.MaxRange) > textParserService.AddressToIntWithSubnet(foundIP))).FirstOrDefault();

                foundDevices.Add(new DevicePing()
                {
                    ID = foundDevices.Count,
                    Name = (found is null) ? AppResources.Device + i : found.Name,
                    IP = foundIP,
                    Description = found?.Entity.Description,
                    Type = (found is null) ? DeviceType.Unknown.ToString() : found.Entity.DevicesGroup.Type,
                    Color = (found is null) ? DeviceType.Unknown.ToString() : found.Entity.DevicesGroup.Color,
                    AnswerTime = $"{sendPing.RoundtripTime}ms",
                });
            }
        }
    }
}
