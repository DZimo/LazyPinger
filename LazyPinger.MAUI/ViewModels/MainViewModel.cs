using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Core.Utils;
using LazyPinger.Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Reflection.Metadata;

namespace LazyPingerMAUI.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<DevicePing> detectedDevices = new();

        [ObservableProperty]
        private ObservableCollection<string> detectedNetworkInterfaces = new();

        [ObservableProperty]
        private string selectedNetworkInterface;

        [ObservableProperty]
        public AnimationHandler animationHandler = new();

        [ObservableProperty]
        public VmUserSelection userSelection;

        [ObservableProperty]
        public ObservableCollection<VmDevicePing> devicesPing;

        private string[] listOfRandomText = ["Lazy Pinger - as fast as possible.", "Welcome to the world of pinging..", "Easy, and convenient", "Customize as you wish", "Are you tired of old pingers 😿"];

        [ObservableProperty]
        public string sloganRandomText = "Lazy Pinger * as fast as possible.";

        //[ObservableProperty]
        //public ObservableCollection<VmDevicesGroup> devicesGroup;

        public IEnumerable<string> ExistingColors { get; set; } = ["Red", "Green", "Blue"];

        public IEnumerable<string> ExistingImages { get; set; } = ["Pcb", "PC", "Phone"];

        public INetworkService NetworkService { get; set; }

        private const int Total_Device_Number = 30000;

        public MainViewModel(INetworkService networkService)
        {
            InitMainVm(networkService);
            _ = InitDatabaseData();

            _ = Task.Run(async () =>
            {
                while (true) {
                    SloganRandomText = listOfRandomText[Random.Shared.Next(listOfRandomText.Count())+1];
                    await Task.Delay(5000);
                }

            });

            NetworkService = networkService;
            //this.networkService = networkService;
        }

        private async Task InitDatabaseData()
        {
            var db = ListenVm.Instance.dbContext;

            var devicesPing = db.DevicePings.Include(o => o.DevicesGroup).ToList().Select( (o) => new VmDevicePing(o)
            { Name = o.Name, 
              Image = o.Image,
              Group = o.DevicesGroup,
              Ip = o?.IP,
            }).ToList();

            DevicesPing = new ObservableCollection<VmDevicePing>(devicesPing);

            var userSelection = db.UserSelections.FirstOrDefault();

            if (userSelection is null)
            {
                db.Add(new UserSelection { AutoRun = true, FastPing = true });
                await db.SaveChangesAsync();
            }

            var res = ListenVm.Instance.DevicesGroupVm.First().Entity;

            userSelection = db.UserSelections.FirstOrDefault();

            if (userSelection is null)
                return;

            UserSelection = new VmUserSelection(userSelection);
        }

        private void InitMainVm(INetworkService networkService)
        {
            MainThread.InvokeOnMainThreadAsync(async () => {
                 ListenVm.LoadAll();
                 await networkService.InitNetworkSettings();
                 var addresses = networkService.NetworkSettings.HostAddresses.Where(o => o.AddressFamily == AddressFamily.InterNetwork).Select(o => o.ToString());
                 DetectedNetworkInterfaces = new ObservableCollection<string>(addresses);
                 //await InitDummyDevices();
            });
        }

        private async Task InitDummyDevices()
        {
            var temp = new DevicePing
            {
                ID = 0,
                Description = "Dummy Device",
                IP = "192.168.0.1",
                MacAddress = "FF:FF:FF:FF",
                Name = "DummyDevice",
                Port = "50",
            };

            for (int i = 0; i < Total_Device_Number; i++) {
                DetectedDevices.Add(temp);
                await Task.Delay(1);
            }
        }

        [RelayCommand]
        public void PingAll(bool isRestart)
        {
            MainThread.InvokeOnMainThreadAsync(async () => {
                if (isRestart)
                    DetectedDevices.Clear();

                await NetworkService.PingAll(ref detectedDevices);
            });
        }

        partial void OnSelectedNetworkInterfaceChanged(string newValue)
        {
            detectedDevices.Clear();

            var res = GetSubnetFromIp(newValue);

            if (res is null)
                return;

            NetworkService.NetworkSettings.SubnetAddress = res;

            MainThread.InvokeOnMainThreadAsync(async () => {
                await NetworkService.PingAll(ref detectedDevices);
            });
        }

        private string? GetSubnetFromIp(string ip)
        {
            var list = ip.Split('.').ToList();
            var subnet = "";
            list.Take(list.Count - 1).ToList().ForEach(o => subnet += $"{o}.");
            return subnet;
        }

    }
}
