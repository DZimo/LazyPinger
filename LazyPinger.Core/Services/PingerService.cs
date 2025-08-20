using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using LazyPinger.Base.Services;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Localization;

namespace LazyPinger.Core.Services
{
    public class PingerService : IPingerService
    {
        private ObservableCollection<Task> pingTasks = new();

        private INetworkService _networkService;

        private IArpDetectorService _arpDetectorService;

        public PingerService(IArpDetectorService arpDetectorService, INetworkService networkService) 
        {
            _arpDetectorService = arpDetectorService;
            _networkService = networkService; 
        }

        public List<DevicePing> GetDevicesOnSubnet()
        {
            throw new NotImplementedException();
        }

        public List<DevicePing> GetDevicesOnSubnet(string ipAddress)
        {
            throw new NotImplementedException();
        }

        public async Task<TcpListener> StartServer(string selectedIP, int selectedPort)
        {
            try
            {
                await Task.Run(() =>
                {
                    IPAddress serverIP = IPAddress.Parse(selectedIP);
                    TcpListener server = new TcpListener(serverIP, selectedPort);
                    server.Start();
                    return server;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(AppResources.StartSerFail + e);
            }
            return null!;
        }

        public Task<bool> PingAll(ref ObservableCollection<DevicePing> foundDevices)
        {
            _ = PingAllAsync(foundDevices);
            return Task.FromResult(true);
        }

        private async Task<bool> PingAllAsync(ObservableCollection<DevicePing> foundDevices)
        {
            PingTaskPoolCreator(128, 256, ref foundDevices);
            await Task.WhenAll(pingTasks);
            return true;
        }

        private void PingTaskPoolCreator(int numberOfTasks, int maxSubNet, ref ObservableCollection<DevicePing> foundDevices)
        {
            var iterator = 1;
            while (iterator < numberOfTasks)
            {
                pingTasks.Add(PingIP((maxSubNet / numberOfTasks) * (iterator - 1), (maxSubNet / numberOfTasks) * iterator, foundDevices));
                iterator++;
            }
        }

        private async Task PingIP(int fromIP, int toIP, ObservableCollection<DevicePing> foundDevices)
        {
            Ping ping = new();

            for (int i = fromIP; i <= toIP; i++)
            {
                if (i == 255)
                    continue;

                byte[] bufferReply = { 87, 69, 66, 85, 73 };
                var ipAddressToPing = _networkService.NetworkSettings.SubnetAddress + i;

                try
                {
                    PingReply sendPing = await ping.SendPingAsync(ipAddressToPing, 1, bufferReply);

                    if (sendPing.Status == IPStatus.Success &&
                        !foundDevices.Select(o => o.IP).ToList().Contains(sendPing.Address.ToString()))
                    {
                        await _arpDetectorService.ArpInit();

                        Console.WriteLine(AppResources.FoundHost + _arpDetectorService.ArpType.Host + " On IP : " + ipAddressToPing);

                        //foundDevices.Add(new DevicePing()
                        //{
                        //    Name = arpDetector.ArpType.Host,
                        //    ID = foundDevices.Count,
                        //    IP = sendPing.Address.ToString(),
                        //    MAC = arpDetector.ArpType.MAC,
                        //    Type = arpDetector.ArpType.Type,
                        //});
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(AppResources.PingIpFail + e);
                }
            }
        }

        public List<String> GetMacAddresses()
        {
            List<String> macAddresses = new();
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                macAddresses.Add(adapter.GetPhysicalAddress().ToString());

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();

                var result = adapterProperties.UnicastAddresses
                    .Select(o => o.Address.ToString())
                    .ToList();

                result.ForEach(Console.WriteLine);
            }

            return macAddresses;
        }

        public bool SendUdpMessage(string selectedIP, string messageToSend, int defaultPort)
        {
            try
            {
                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress addressToSend = IPAddress.Parse(selectedIP);
                IPEndPoint endPoint = new IPEndPoint(addressToSend, defaultPort);

                byte[] msgBuffer = Encoding.ASCII.GetBytes(messageToSend);

                udpSocket.SendTo(msgBuffer, endPoint);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool SendUdpBroadcast(string selectedIP, string messageToSend, int defaultPort)
        {
            try
            {
                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress addressToSend = IPAddress.Parse(selectedIP);
                IPEndPoint endPoint = new IPEndPoint(addressToSend, defaultPort);
                udpSocket.EnableBroadcast = true;

                byte[] msgBuffer = Encoding.ASCII.GetBytes(messageToSend);

                udpSocket.SendTo(msgBuffer, endPoint);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> SendTCPFile(string selectedIP, string messageToSend, int defaultPort)
        {
            try
            {
                using Socket TcpSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                await TcpSocket.ConnectAsync(selectedIP, defaultPort);
                await TcpSocket.SendFileAsync("wwwroot/DataTest.json");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

    }
}
