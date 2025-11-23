using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Common;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.User;
using LazyPinger.Core.Services;
using LazyPinger.Core.Utils;
using LazyPinger.Core.ViewModels;
using LazyPinger.MAUI.Views.CAN;
using LazyPingerMAUI.Views;
using LazyPingerMAUI.Views.TCP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Maui.Media;
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
        public string quickSettingsText = "Hide Quick Settings";

        //[ObservableProperty]
        //public ObservableCollection<VmDevicesGroup> devicesGroup;

        public IEnumerable<string> ExistingColors { get; set; } = ["Red", "Green", "Blue"];

        public IEnumerable<string> ExistingImages { get; set; } = ["Pcb", "PC", "Phone"];

        public INetworkService NetworkService { get; set; }

        private const int Total_Device_Number = 30000;

        [ObservableProperty]
        private bool isPingIdle = true;

        [ObservableProperty]
        private bool isQuickSettingsExpanded = true;

        private ITextParserService textParserService;

        public MainPage _MainPage { get; set; }

        public MainViewModel(INetworkService networkService, ITextParserService textParserService)
        {
            InitMainVm(networkService);

            _ = Task.Run(async () =>
            {
                await InitDatabaseData();
                AutoRestart();

                while (true)
                {
                    var rand = listOfRandomText[Random.Shared.Next(listOfRandomText.Count())];

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            SloganRandomText = rand;
                        }
                        catch (Exception ex)
                        {
                            LazyLogger.LogAll(ex.Message, LogSeverity.Error);
                        }
                    });
                    await Task.Delay(5000);
                }
            });

            NetworkService = networkService;
            this.textParserService = textParserService;
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

                    catch(Exception ex)
                    {
                        LazyLogger.LogAll(ex.Message, LogSeverity.Error);
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
                    LazyLogger.LogAll(ex.Message, LogSeverity.Error);
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
                LazyLogger.LogAll(ex.Message, LogSeverity.Error);
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
                    LazyLogger.LogAll(ex.Message, LogSeverity.Error);
                }
                finally
                {
                    IsPingIdle = true;
                }
            });
        }

        [RelayCommand]
        public async Task LoadView(string id)
        {
            switch (id)
            {
                case "0":
                    await _MainPage.Navigation.PushAsync(App.Services.GetService<PingView>());
                    break;
                case "1":
                    await _MainPage.Navigation.PushAsync(App.Services.GetService<NetworkPage>());
                    break;
                case "2":
                    await _MainPage.Navigation.PushAsync(App.Services.GetService<CanPage>());
                    break;
                case "3":
                    await _MainPage.Navigation.PushAsync(App.Services.GetService<SettingsPage>());
                    break;
                default:
                    break;
            }
        }
        

        partial void OnSelectedNetworkInterfaceChanged(string value)
        {
            NetworkService.NetworkSettings.IpAddress = value;

            detectedDevices.Clear();

            var res = textParserService.GetSubnetFromAddress(value);

            if (res is null)
                return;

            NetworkService.NetworkSettings.SubnetAddress = res;

            if (ListenVm.Instance.UserSelectionsVm is not null && !ListenVm.Instance.UserSelectionsVm.IsAutoRunEnabled)
                return;

            PingAll(true);
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
