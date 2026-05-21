using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models.CAN;
using System.Collections.ObjectModel;

namespace LazyPinger.Core.ViewModels;

public partial class VmCanBus : ViewModelBase
{
    [ObservableProperty]
    private string canInterface = string.Empty;

    [ObservableProperty]
    private CanBaudRate selectedBaudRate = CanBaudRate.Baud500K;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string statusColor = "#FF0000";

    [ObservableProperty]
    private string statusText = "Disconnected";

    [ObservableProperty]
    private ObservableCollection<CanMessage> receivedMessages = [];

    [ObservableProperty]
    private int messageCount;

    [ObservableProperty]
    private ObservableCollection<string> availableInterfaces = [];

    [ObservableProperty]
    private string? selectedInterface;
}
