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

        [ObservableProperty]
        private bool isPingIdle = true;

        public MainViewModel(INetworkService networkService)
        {
            InitMainVm(networkService);

            _ = Task.Run(async () =>
            {
                await InitDatabaseData();
                AutoRestart();

                while (true) {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        SloganRandomText = listOfRandomText[Random.Shared.Next(listOfRandomText.Count())];
                    });
                    await Task.Delay(5000);
                }
            });

            NetworkService = networkService;
        }

        private void AutoRestart()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (ListenVm.Instance.UserSelectionsVm is null)
                    {
                        await Task.Delay(5000);
                        continue;
                    }

                    if (!ListenVm.Instance.UserSelectionsVm.IsAutoRestartEnabled)
                    {
                        await Task.Delay(5000);
                        continue;
                    }

                    try
                    {
                        PingAll(true);
                    }

                    catch
                    {

                    }

                    if (ListenVm.Instance.UserSelectionsVm.Entity.AutoRestartTime < 10)
                        ListenVm.Instance.UserSelectionsVm.Entity.AutoRestartTime = 1000;

                    await Task.Delay(ListenVm.Instance.UserSelectionsVm.Entity.AutoRestartTime);
                }
            });
        }

        private async Task InitDatabaseData()
        {
            var db = ListenVm.Instance.dbContext;

            try {

                _ = Task.Run(() =>
                {
                    var devicesPing = db.DevicePings.Include(o => o.DevicesGroup).ToList().Select((o) => new VmDevicePing(o)
                    {
                        Name = o.Name,
                        Image = o.Image,
                        Group = o.DevicesGroup,
                        Ip = o?.IP,
                    }).ToList();

                    DevicesPing = new ObservableCollection<VmDevicePing>(devicesPing);
                });


                _ = Task.Run(async () =>
                {
                    var userSelection = db.UserSelections.FirstOrDefault();

                    if (userSelection is not null)
                        ListenVm.Instance.UserSelectionsVm = new VmUserSelection(userSelection);

                    if (userSelection is null)
                    {
                        var user = new UserSelection { AutoRun = true, FastPing = true, AutoRestart = true, AutoRestartTime = 1000 };
                        db.Add(user);
                        ListenVm.Instance.UserSelectionsVm = new VmUserSelection(user);
                        await db.SaveChangesAsync();
                    }
                });


                _ = Task.Run(() =>
                {
                    var res = ListenVm.Instance.DevicesGroupVm.First().Entity;

                    var userSelection = db.UserSelections.FirstOrDefault();

                    if (userSelection is null)
                        return;

                    UserSelection = new VmUserSelection(userSelection);
                });
            }
            catch { }
        }

        private void InitMainVm(INetworkService networkService)
        {
            MainThread.InvokeOnMainThreadAsync( async () => {
                ListenVm.LoadAll();
                 await networkService.InitNetworkSettings();
                 var addresses = networkService.NetworkSettings.HostAddresses.Where(o => o.AddressFamily == AddressFamily.InterNetwork).Select(o => o.ToString());
                 DetectedNetworkInterfaces = new ObservableCollection<string>(addresses);
                 //await InitDummyDevices();
            });
        }

        [RelayCommand]
        public void PingAll(bool isRestart)
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                IsPingIdle = false;

                if (isRestart)
                    DetectedDevices.Clear();

                try
                {
                    await NetworkService.PingAll(DetectedDevices);
                    OrderDevices();
                }
                catch
                {

                }
                finally
                {
                    IsPingIdle = true;
                }
            });
        }

        partial void OnSelectedNetworkInterfaceChanged(string value)
        {
            if (ListenVm.Instance.UserSelectionsVm is not null && !ListenVm.Instance.UserSelectionsVm.IsAutoRunEnabled)
                return;

            detectedDevices.Clear();

            var res = GetSubnetFromIp(value);

            if (res is null)
                return;

            NetworkService.NetworkSettings.SubnetAddress = res;

            PingAll(true);
        }

        private string? GetSubnetFromIp(string ip)
        {
            var list = ip.Split('.').ToList();
            var subnet = "";
            list.Take(list.Count - 1).ToList().ForEach(o => subnet += $"{o}.");
            return subnet;
        }

        private void OrderDevices()
        {
            var order = new ObservableCollection<DevicePing>(DetectedDevices.OrderBy(o => o.IP));
            DetectedDevices.Clear();
            order.ToList().ForEach(o => { DetectedDevices.Add(o); });
        }

    }
}
