using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Core.ViewModels;

namespace LazyPingerMAUI.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        public MainViewModel MainVm { get; set; }

        public DevicePing devicePingTemp { get; set; } = new();

        public DevicePing DevicePingTemp 
        {
            get
            {
                var res = devicePingTemp.Validate(null);

                if (res.Count() > 0)
                {
                    // Notification
                    return devicePingTemp;
                }
                CanCreateDevice = true;

                return devicePingTemp;
            }
            set
            {
                devicePingTemp = value;
            }
        }


        private DevicesGroup deviceGroupTemp = new();
        public DevicesGroup DeviceGroupTemp 
        {
            get 
            {
                var res = deviceGroupTemp.Validate(null);

                if (res.Count() > 0)
                {
                    // Notification
                    return deviceGroupTemp;
                }
                CanCreateDeviceGroup = true;

                return deviceGroupTemp;
            }
            set
            {
                deviceGroupTemp = value;
            }
        }

        public VmDevicesGroup VmDeviceGroupTemp { get; set; }

        [ObservableProperty]
        public bool canCreateDevice;

        [ObservableProperty]
        public bool canCreateDeviceGroup;

        public SettingsViewModel(INetworkService networkService, MainViewModel mainViewModel)
        {
            MainVm = mainViewModel;
        }

        [RelayCommand]
        public async Task CreateNewDevice()
        {

            var db = ListenVm.Instance.dbContext;
            var newDevice = new DevicePing()
            {
                Name = DevicePingTemp.Name,
                DevicesGroup = VmDeviceGroupTemp.Entity,
                Image = DevicePingTemp.Image,
                IP = DevicePingTemp.IP,
            };

            MainVm.DevicesPing.Add(new VmDevicePing(newDevice));

            try
            {
                db.DevicePings.Add(newDevice);
                await db.SaveChangesAsync();
                ListenVm.ReloadFromDatabase(newDevice);
            }

            catch (Exception ex) {

            }
        }

        [RelayCommand]
        public async Task CreateNewDeviceGroup()
        {
            var db = ListenVm.Instance.dbContext;

            try
            {
                db.DevicesGroups.Add( new() { Color = DeviceGroupTemp.Color, Type = DeviceGroupTemp.Type });
                await db.SaveChangesAsync();
                ListenVm.ReloadFromDatabase(DeviceGroupTemp);
            }

            catch (Exception ex)
            {

            }
        }


        [RelayCommand]
        public async Task ApplyUserSelection()
        {
            try
            {
                var res = ListenVm.Instance.UserSelectionsVm?.Entity;

                if (res is null)
                    return;

                ListenVm.Instance.dbContext.Update(res);
                await ListenVm.Instance.dbContext.SaveChangesAsync();
            }
            catch
            {

            }
        }
    }
}
