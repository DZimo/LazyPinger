using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LazyPinger.Base.IServices
{
    public interface ITextParserService
    {
        public string? GetSubnetFromAddress(string? address);

        public string GetAddressFromSubnet(string ip);

        public int AddressToInt(string address);

        public int AddressToIntWithSubnet(string address);

        public long AddressToLong(string address, bool isReversed = false);
    }
}
