using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Network;
using System.Collections.ObjectModel;

namespace LazyPingerMAUI.ViewModels
{
    public partial class CanViewModel : ViewModelBase, IDisposable
    {
        private readonly ICanService _canService;
        private bool _disposed;

        [ObservableProperty]
        private ObservableCollection<CanFrame> canFrames = [];

        [ObservableProperty]
        private ObservableCollection<string> availableInterfaces = [];

        [ObservableProperty]
        private string? selectedInterface;

        [ObservableProperty]
        private ObservableCollection<string> baudRateOptions = new(
            Enum.GetValues<CanBaudRate>()
                .Select(FormatBaudRate));

        [ObservableProperty]
        private string selectedBaudRateOption = "500 kbit/s";

        [ObservableProperty]
        private bool isConnected;

        [ObservableProperty]
        private string statusColor = "#FF0000";

        [ObservableProperty]
        private string statusText = "Disconnected";

        [ObservableProperty]
        private int messageCount;

        public CanViewModel(ICanService canService)
        {
            _canService = canService;
            _canService.MessageReceived += OnMessageReceived;
            RefreshInterfaces();
        }

        [RelayCommand]
        public void RefreshInterfaces()
        {
            var interfaces = _canService.GetAvailableInterfaces();
            AvailableInterfaces = new ObservableCollection<string>(interfaces);

            if (interfaces.Count > 0 && SelectedInterface is null)
                SelectedInterface = interfaces[0];
        }

        [RelayCommand]
        public async Task OpenCanSocket()
        {
            if (string.IsNullOrWhiteSpace(SelectedInterface))
                return;

            var baudRate = ParseBaudRate(SelectedBaudRateOption);
            var success = await _canService.OpenAsync(SelectedInterface, baudRate);

            if (success)
            {
                IsConnected = true;
                StatusColor = "#00FF00";
                StatusText = $"Connected to {SelectedInterface}";
            }
            else
            {
                StatusText = "Failed to open CAN socket";
            }
        }

        [RelayCommand]
        public void CloseCanSocket()
        {
            _canService.Close();
            IsConnected = false;
            StatusColor = "#FF0000";
            StatusText = "Disconnected";
        }

        [RelayCommand]
        public void ClearMessages()
        {
            CanFrames.Clear();
            MessageCount = 0;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _canService.MessageReceived -= OnMessageReceived;
            _disposed = true;
        }

        private void OnMessageReceived(object? sender, CanFrame frame)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CanFrames.Insert(0, frame);
                MessageCount = CanFrames.Count;

                while (CanFrames.Count > 10000)
                    CanFrames.RemoveAt(CanFrames.Count - 1);
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
}
