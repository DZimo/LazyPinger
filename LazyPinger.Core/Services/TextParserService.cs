using LazyPinger.Base.IServices;
using System.Net;

namespace LazyPinger.Core.Services
{
    public class TextParserService : ITextParserService
    {
        public string GetSubnetFromAddress(string ip)
        {
            var list = ip.Split('.').ToList();
            var subnet = "";
            list.Take(list.Count - 1).ToList().ForEach(o => subnet += $"{o}.");
            return subnet;
        }

        public string GetAddressFromSubnet(string ip)
        {
            var list = ip.Split('.').ToList();
            var address = "";
            address = list.Last().ToString();
            return address;
        }

        public int AddressToInt(string address)
        {
            return Int16.Parse(address);
        }

        public int AddressToIntWithSubnet(string address)
        {
            var res = GetAddressFromSubnet(address);
            return Int16.Parse(res);
        }

        public long AddressToLong(string address, bool isReversed = false)
        {
            var addressBytes = IPAddress.Parse(address).GetAddressBytes();

            if (isReversed)
                addressBytes.Reverse();

            return BitConverter.ToUInt32(addressBytes, 0);
        }
    }
}
