using System.ComponentModel.DataAnnotations;
using System.Net;

namespace LazyPinger.Base.Models.Network
{
    public class NetworkSettings
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string IpAddress { get; set; }

        public List<IPAddress> HostAddresses { get; set; } = new();

        private string subnetAddress = string.Empty;

        public string SubnetAddress 
        {
            get => subnetAddress;
            set
            {
                subnetAddress = value;
            }
        }

        public string GatewayAddress { get; set; } = string.Empty;


        public string MacDictionaryFile = "ListofMAC.txt";

        public int MinSubnetRange { get; set; } = 0;

        public int MaxSubnetRange { get; set; } = 255;

        public int PingTimeout { get; set; } = 1000;

    }
}
