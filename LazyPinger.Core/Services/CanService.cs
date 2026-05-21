using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Network;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LazyPinger.Core.Services
{
    public class CanService : ICanService
    {
        private const int AF_CAN = 29;
        private const int PF_CAN = AF_CAN;
        private const int SOCK_RAW = 3;
        private const int CAN_RAW = 1;
        private const int SIOCGIFINDEX = 0x8933;
        private const int CAN_MTU = 16;
        private const uint CAN_EFF_FLAG = 0x80000000;
        private const uint CAN_EFF_MASK = 0x1FFFFFFF;
        private const uint CAN_SFF_MASK = 0x000007FF;

        private int _socketHandle = -1;
        private CancellationTokenSource? _listenerCts;
        private Task? _listenerTask;

        public bool IsConnected => _socketHandle >= 0;

        public string? ConnectedInterface { get; private set; }

        public event EventHandler<CanFrame>? MessageReceived;

        public List<string> GetAvailableInterfaces()
        {
            var interfaces = new List<string>();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return interfaces;

            try
            {
                var netDir = "/sys/class/net";
                if (!Directory.Exists(netDir))
                    return interfaces;

                foreach (var dir in Directory.GetDirectories(netDir))
                {
                    var typePath = Path.Combine(dir, "type");
                    if (!File.Exists(typePath))
                        continue;

                    var typeContent = File.ReadAllText(typePath).Trim();
                    if (typeContent == "280")
                        interfaces.Add(Path.GetFileName(dir));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enumerate CAN interfaces: {ex.Message}");
            }

            return interfaces;
        }

        public async Task<bool> OpenAsync(string interfaceName, CanBaudRate baudRate)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return false;

            Close();

            try
            {
                if (!await ConfigureInterfaceAsync(interfaceName, baudRate))
                    return false;

                _socketHandle = LinuxNative.Socket(PF_CAN, SOCK_RAW, CAN_RAW);
                if (_socketHandle < 0)
                    return false;

                var ifr = new LinuxNative.Ifreq();
                var nameBytes = System.Text.Encoding.ASCII.GetBytes(interfaceName);
                Array.Copy(nameBytes, ifr.Name, Math.Min(nameBytes.Length, 15));

                if (LinuxNative.Ioctl(_socketHandle, SIOCGIFINDEX, ref ifr) < 0)
                {
                    CloseSocket();
                    return false;
                }

                var addr = new LinuxNative.SockAddrCan
                {
                    Family = AF_CAN,
                    InterfaceIndex = ifr.InterfaceIndex
                };

                if (LinuxNative.Bind(_socketHandle, ref addr, Marshal.SizeOf<LinuxNative.SockAddrCan>()) < 0)
                {
                    CloseSocket();
                    return false;
                }

                ConnectedInterface = interfaceName;
                StartListener();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open CAN socket: {ex.Message}");
                CloseSocket();
                return false;
            }
        }

        public void Close()
        {
            CloseSocket();
            StopListener();
            ConnectedInterface = null;
        }

        private static async Task<bool> ConfigureInterfaceAsync(string interfaceName, CanBaudRate baudRate)
        {
            try
            {
                await RunCommandAsync("ip", $"link set {interfaceName} down");

                var result = await RunCommandAsync(
                    "ip",
                    $"link set {interfaceName} type can bitrate {(int)baudRate}");

                if (result != 0)
                    return false;

                result = await RunCommandAsync("ip", $"link set {interfaceName} up");
                return result == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to configure CAN interface: {ex.Message}");
                return false;
            }
        }

        private static async Task<int> RunCommandAsync(string command, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
                return -1;

            await process.WaitForExitAsync();
            return process.ExitCode;
        }

        private void StartListener()
        {
            _listenerCts = new CancellationTokenSource();
            var token = _listenerCts.Token;
            var socketFd = _socketHandle;

            _listenerTask = Task.Run(() =>
            {
                var buffer = new byte[CAN_MTU];

                while (!token.IsCancellationRequested && socketFd >= 0)
                {
                    var bytesRead = LinuxNative.Read(socketFd, buffer, CAN_MTU);
                    if (bytesRead < CAN_MTU)
                        break;

                    var canId = BitConverter.ToUInt32(buffer, 0);
                    var dlc = buffer[4];
                    var isExtended = (canId & CAN_EFF_FLAG) != 0;

                    var rawId = isExtended
                        ? canId & CAN_EFF_MASK
                        : canId & CAN_SFF_MASK;

                    var data = new byte[dlc];
                    Array.Copy(buffer, 8, data, 0, Math.Min((int)dlc, 8));

                    var frame = new CanFrame
                    {
                        Id = rawId,
                        Data = data,
                        DataLength = dlc,
                        IsExtendedId = isExtended,
                        Timestamp = DateTime.Now
                    };

                    MessageReceived?.Invoke(this, frame);
                }
            }, token);
        }

        private void StopListener()
        {
            _listenerCts?.Cancel();

            try
            {
                _listenerTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Task may have faulted after socket close
            }

            _listenerTask = null;
            _listenerCts?.Dispose();
            _listenerCts = null;
        }

        private void CloseSocket()
        {
            if (_socketHandle >= 0)
            {
                LinuxNative.Close(_socketHandle);
                _socketHandle = -1;
            }
        }

        private static class LinuxNative
        {
            [DllImport("libc", EntryPoint = "socket", SetLastError = true)]
            public static extern int Socket(int domain, int type, int protocol);

            [DllImport("libc", EntryPoint = "bind", SetLastError = true)]
            public static extern int Bind(int sockfd, ref SockAddrCan addr, int addrlen);

            [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
            public static extern int Ioctl(int fd, uint request, ref Ifreq ifr);

            [DllImport("libc", EntryPoint = "read", SetLastError = true)]
            public static extern int Read(int fd, byte[] buf, int count);

            [DllImport("libc", EntryPoint = "close", SetLastError = true)]
            public static extern int Close(int fd);

            [StructLayout(LayoutKind.Sequential)]
            public struct SockAddrCan
            {
                public short Family;
                public int InterfaceIndex;
                public uint RxId;
                public uint TxId;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct Ifreq
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] Name;

                public int InterfaceIndex;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] Padding;

                public Ifreq()
                {
                    Name = new byte[16];
                    Padding = new byte[20];
                }
            }
        }
    }
}
