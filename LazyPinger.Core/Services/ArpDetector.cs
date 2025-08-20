using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Network;
using LazyPinger.Base.Services;
using LazyPinger.Core.IPHlp;
using LazyPinger.Core.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace LazyPinger.Core.Services;

public class ArpDetectorService : IArpDetectorService
{
    private List<string> MacList = new();
    public ArpType ArpType { get; set; } = new();

    private INetworkService _networkService;

    private string IpAddressToPing;


    public ArpDetectorService(INetworkService networkService)
    {
        _networkService = networkService;

        MacList = MacLoader(_networkService.NetworkSettings.MacDictionaryFile);
        IpAddressToPing = _networkService.NetworkSettings.IpAddress;
    }
    
    public async Task ArpInit()
    {
        ArpType.MAC = MacFinder();
        ArpType.Host = await HostFinder();
        ArpType.Type = TypeFinder();
    }

    private string TypeFinder()
    {
        string foundType = "Unknown";
        string pattern = MacFinder().Substring(0, 8).Replace("-", ":") + ".*";

        MacList.ForEach(o =>
        {
            Match found = Regex.Match(o, pattern);
            if (found.Success)
                foundType = found.Value.Split(" ")[9];
        });

        return foundType;
    }

    
    private async Task<string> HostFinder()
    {
        try
        {
            Console.WriteLine("IP HOST FINDER : "+ IpAddressToPing);

            var foundIP = IpAddressToPing;
            var res = Dns.GetHostByAddress(foundIP);
            return res.HostName;
        }
        catch (Exception e)
        {

        }

        return "Unknown";
    }
    
    private List<string> MacLoader(string filename)
    {
        List<string> list = new();
        // MAUI CHANGE GetCurrentDirectory()

        //var res = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, filename);
        var res = Path.Combine(System.IO.Directory.GetCurrentDirectory(), filename);
        File.ReadAllLines(res).ToList().ForEach(o => list.Add(o.Trim()));
        return list;
    }
    
    private string MacFinder()
    {
        uint macAddrLen = (uint)new byte[6].Length;
        IPAddress ipAddress = new IPAddress(0);
        ipAddress = IPAddress.Parse(IpAddressToPing);
        byte[] macAddr = new byte[6];
            
        IPHlpAPI32Wrapper.SendARP((int)BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen);
        return BitConverter.ToString(macAddr);
    }
}