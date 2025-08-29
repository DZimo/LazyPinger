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

        [ObservableProperty]
        private bool isIpBased = true;

        public DevicePing devicePingTemp { get; set; } = new();

        public DevicePing DevicePingTemp 
        {
            get
            {
                var res = devicePingTemp.Validate(null);

                if (res.Count() > 0)
                {
                    //CanCreateDevice = false;
                    // Notification
                    return devicePingTemp;
                }
                CanCreateDevice = true;

                return devicePingTemp;
            }
            set
            {
                var res = devicePingTemp.Validate(null);

                if (res.Count() > 0)
                {
                    CanCreateDevice = false;
                }

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
                    //CanCreateDeviceGroup = false;
                    // Notification
                    return deviceGroupTemp;
                }
                CanCreateDeviceGroup = true;

                return deviceGroupTemp;
            }
            set
            {
                var res = deviceGroupTemp.Validate(null);

                if (res.Count() > 0)
                {
                    CanCreateDeviceGroup = false;
                }

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
            if (ListenVm.Instance.UserSelectionsVm is null)
                return;

            var db = ListenVm.Instance.dbContext;
            var newDevice = new DevicePing()
            {
                Name = DevicePingTemp.Name,
                DevicesGroup = VmDeviceGroupTemp.Entity,
                Image = [0, 2],
                IP = DevicePingTemp.IP,
                UserSelectionID = ListenVm.Instance.UserSelectionsVm.EntityID,
            };

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
            if (ListenVm.Instance.UserSelectionsVm is null)
                return;

            var db = ListenVm.Instance.dbContext;

            var newDevicesGroup = new DevicesGroup()
            {
                Color = DeviceGroupTemp.Color,
                Type = DeviceGroupTemp.Type,
                UserSelectionID = ListenVm.Instance.UserSelectionsVm.EntityID,
            };

            try
            {
                db.DevicesGroups.Add(newDevicesGroup);
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
