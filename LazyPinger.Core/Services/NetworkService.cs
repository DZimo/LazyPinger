using LazyPinger.Base.IServices;
using LazyPinger.Base.Localization;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.Network;
using LazyPinger.Core.ViewModels;
using System.Collections.ObjectModel;
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

        public NetworkService()
        {

        }

        public async Task InitNetworkSettings()
        {
            var res = await GetHostIPs();
            NetworkSettings.HostAddresses = res.ToList();
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

        public async Task<TcpListener?> StartServer(string selectedIP, int selectedPort)
        {
            await Task.Run(() =>
            {
                var serverIP = IPAddress.Parse(selectedIP);
                var server = new TcpListener(serverIP, selectedPort);
                server.Start();
                return server;
            });
            return null;
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

        public bool SendUDP(string selectedIP, string msg, int defaultPort, bool broadcast = false)
        {
            try
            {
                var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var addressToSend = IPAddress.Parse(selectedIP);
                var endPoint = new IPEndPoint(addressToSend, defaultPort);
                udpSocket.EnableBroadcast = broadcast;

                byte[] msgBuffer = Encoding.ASCII.GetBytes(msg);

                udpSocket.SendTo(msgBuffer, endPoint);
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

                byte[] bufferReply = { 00 };
                var ipAddressToPing = NetworkSettings.SubnetAddress + i;

                var sendPing = await ping.SendPingAsync(ipAddressToPing, NetworkSettings.PingTimeout, bufferReply);

                if (sendPing.Status != IPStatus.Success)
                    return;

                var foundIP = sendPing.Address.ToString();
                //var arpDetector = new(ipAddressToPing)
                //await ArpDetectorService.ArpInit();

                var devicesPings = ListenVm.Instance.DevicesPing;

                if (foundDevices.ToList().Exists(o => o.IP == foundIP))
                    continue;

                var found = devicesPings.Where(o => o.IP == foundIP).FirstOrDefault();

                foundDevices.Add(new DevicePing()
                {
                    ID = foundDevices.Count,
                    Name = (found is null ) ? AppResources.Device+ i : found.Name,
                    IP = foundIP,
                    Description = found?.Description,
                    MacAddress = "XX:XX:XX:XX:XX",
                    Type = (found is null )? DeviceType.Unknown.ToString() : found.DevicesGroup.Type,
                    Color = (found is null) ? DeviceType.Unknown.ToString() : found.DevicesGroup.Color,
                    Port = "-",
                    AnswerTime = $"{sendPing.RoundtripTime}ms",
                });
            }
        }
    }
}
