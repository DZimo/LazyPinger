using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyPinger.Base.IServices
{
    public interface ITextParserService
    {
        public string GetSubnetFromAddress(string address);

        public string GetAddressFromSubnet(string ip);

        public int AddressToInt(string address);

        public int AddressToIntWithSubnet(string address);
        
    }
}
