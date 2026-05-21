using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.CAN;
using LazyPinger.Core.ViewModels;
using System.Collections.ObjectModel;

namespace LazyPingerMAUI.ViewModels;

public partial class CanViewModel : ViewModelBase
{
    private readonly ICanService _canService;

    [ObservableProperty]
    private VmCanBus vmCanBus = new();

    [ObservableProperty]
    private ObservableCollection<string> baudRateOptions = new(
        Enum.GetValues<CanBaudRate>()
            .Select(b => FormatBaudRate(b)));

    [ObservableProperty]
    private string selectedBaudRateOption = "500 kbit/s";

    public CanViewModel(ICanService canService)
    {
        _canService = canService;
        _canService.MessageReceived += OnCanMessageReceived;

        RefreshInterfaces();
    }

    [RelayCommand]
    public void RefreshInterfaces()
    {
        var interfaces = _canService.GetAvailableInterfaces();
        VmCanBus.AvailableInterfaces = new ObservableCollection<string>(interfaces);

        if (interfaces.Count > 0 && VmCanBus.SelectedInterface is null)
            VmCanBus.SelectedInterface = interfaces[0];
    }

    [RelayCommand]
    public async Task OpenCanSocket()
    {
        if (string.IsNullOrWhiteSpace(VmCanBus.SelectedInterface))
            return;

        var baudRate = ParseBaudRate(SelectedBaudRateOption);
        var success = await _canService.OpenAsync(VmCanBus.SelectedInterface, baudRate);

        if (success)
        {
            VmCanBus.IsConnected = true;
            VmCanBus.StatusColor = "#00FF00";
            VmCanBus.StatusText = $"Connected to {VmCanBus.SelectedInterface}";
        }
        else
        {
            VmCanBus.StatusText = "Failed to open CAN socket";
        }
    }

    [RelayCommand]
    public void CloseCanSocket()
    {
        _canService.Close();
        VmCanBus.IsConnected = false;
        VmCanBus.StatusColor = "#FF0000";
        VmCanBus.StatusText = "Disconnected";
    }

    [RelayCommand]
    public void ClearMessages()
    {
        VmCanBus.ReceivedMessages.Clear();
        VmCanBus.MessageCount = 0;
    }

    private void OnCanMessageReceived(object? sender, CanMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            VmCanBus.ReceivedMessages.Insert(0, message);
            VmCanBus.MessageCount = VmCanBus.ReceivedMessages.Count;

            // Keep buffer bounded to avoid memory issues
            while (VmCanBus.ReceivedMessages.Count > 10000)
                VmCanBus.ReceivedMessages.RemoveAt(VmCanBus.ReceivedMessages.Count - 1);
        });
    }

    private static string FormatBaudRate(CanBaudRate baudRate)
    {
        var value = (int)baudRate;
        return value >= 1000000
            ? $"{value / 1000000} Mbit/s"
            : $"{value / 1000} kbit/s";
    }

    private static CanBaudRate ParseBaudRate(string option)
    {
        foreach (var baudRate in Enum.GetValues<CanBaudRate>())
        {
            if (FormatBaudRate(baudRate) == option)
                return baudRate;
        }
        return CanBaudRate.Baud500K;
    }
}
