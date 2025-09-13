using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;

namespace LazyPinger.Core.ViewModels
{
    public partial class VmNetworkUser : ViewModelBase
    {
        [ObservableProperty]
        private int tcpPort;

        [ObservableProperty]
        private int udpPort;

        [ObservableProperty]
        private string receivedTcpMessage = string.Empty;

        [ObservableProperty]
        private string receivedUdpMessage = string.Empty;

        [ObservableProperty]
        private string sentTcpMessage = string.Empty;

        [ObservableProperty]
        private string sentUdpMessage = string.Empty;
    }
}
