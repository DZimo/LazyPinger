using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.User;
using LazyPinger.Core.Utils;
using LazyPinger.Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Net.Sockets;

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

        private string[] listOfRandomText = ["Lazy Pinger", "As fast as possible ", "Welcome to pinging", "Easy, and convenient", "Customize as you wish", "Tired of old tools"];

        [ObservableProperty]
        public string sloganRandomText = "Lazy Pinger";

        [ObservableProperty]
        public string quickSettingsText = "Expand Quick Settings";

        //[ObservableProperty]
        //public ObservableCollection<VmDevicesGroup> devicesGroup;

        public IEnumerable<string> ExistingColors { get; set; } = ["Red", "Green", "Blue"];

        public IEnumerable<string> ExistingImages { get; set; } = ["Pcb", "PC", "Phone"];

        public INetworkService NetworkService { get; set; }

        private const int Total_Device_Number = 30000;

        [ObservableProperty]
        private bool isPingIdle = true;

        [ObservableProperty]
        private bool isQuickSettingsExpanded = false;

        public MainViewModel(INetworkService networkService)
        {
            InitMainVm(networkService);

            _ = Task.Run(async () =>
            {
                await InitDatabaseData();
                AutoRestart();

                while (true) {
                    var rand = listOfRandomText[Random.Shared.Next(listOfRandomText.Count())];

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            SloganRandomText = rand;
                        }
                        catch(Exception ex)
                        {

                        }
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
            try {
                var db = ListenVm.Instance.dbContext;

                try
                {
                    await db.Database.MigrateAsync();
                    ListenVm.Instance.dbLockSemaphore.Release();
                }
                catch (Exception ex)
                {

                }

                await Task.Run(async () =>
                {
                    await ListenVm.Instance.dbLockSemaphore.WaitAsync();

                    var res = db.Database.GetDbConnection().DataSource;
                    var userPreference = await db.UserPreferences.FirstOrDefaultAsync();

                    if (userPreference is not null)
                        return;

                    var userSelectionTemp = new UserSelection { AutoRun = true, FastPing = true, AutoRestart = true, AutoRestartTime = 10000 };
                    var userPreferenceTemp = new UserPreference { Name = "Default Preference", UserSelection = userSelectionTemp };

                    db.Add(userPreferenceTemp);

                    var devicesGroupDefault = new DevicesGroup()
                    {
                        Type = "Unknown",
                        Color = "Yellow",
                        UserSelection = userSelectionTemp,
                    };

                    db.Add(devicesGroupDefault);
                    ListenVm.Instance.dbLockSemaphore.Release();
                });


                //await Task.Run(async () =>
                //{
                //    await ListenVm.Instance.dbLockSemaphore.WaitAsync();


                //    if (db.DevicePings is not null)
                //        return;

                //    var devicesPingDb = db.DevicePings?.Include(o => o.DevicesGroup).ToList().Select((o) => new VmDevicePing(o)
                //    {
                //        Name = o.Name,
                //        Image = o?.Image,
                //        Group = new VmDevicesGroup(o.DevicesGroup),
                //        Ip = o?.IP,
                //    }).ToList();

                //    if (devicesPingDb is null)
                //        return;

                //    DevicesPing = new ObservableCollection<VmDevicePing>(devicesPingDb);
                //    ListenVm.Instance.dbLockSemaphore.Release();
                //});

                await db.SaveChangesAsync();
                ListenVm.ReloadAllFromDatabase();
            }
            catch (Exception ex) {
                //
            }
        }

        private void InitMainVm(INetworkService networkService)
        {
            MainThread.InvokeOnMainThreadAsync( async () => {
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
                if (NetworkService.NetworkSettings.SubnetAddress is null)
                    return;

                IsPingIdle = false;

                if (isRestart)
                    DetectedDevices.Clear();

                try
                {
                    await NetworkService.PingAll(DetectedDevices);
                    OrderDevices();
                }
                catch (Exception ex)
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
            detectedDevices.Clear();

            var res = GetSubnetFromIp(value);

            if (res is null)
                return;

            NetworkService.NetworkSettings.SubnetAddress = res;

            if (ListenVm.Instance.UserSelectionsVm is not null && !ListenVm.Instance.UserSelectionsVm.IsAutoRunEnabled)
                return;

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

        partial void OnIsQuickSettingsExpandedChanged(bool oldValue, bool newValue)
        {
            QuickSettingsText = newValue ? "Hide Quick Settings" : "Expand Quick Settings";
        }

    }
}
