using LazyPinger.Base.IServices;
using LazyPinger.Base.Localization;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.Network;
using LazyPinger.Core.ViewModels;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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
            var ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());
            return ipHostInfo.AddressList;
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

        public Task<bool> PingAll(ref ObservableCollection<DevicePing> foundDevices)
        {
            _ = PingAllAsync(foundDevices);
            return Task.FromResult(true);
        }

        public List<string> GetMacAddresses()
        {
            throw new NotImplementedException();
        }

        public bool SendUdpMessage(string selectedIP, string messageToSedn, int defaultPort)
        {
            throw new NotImplementedException();
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
