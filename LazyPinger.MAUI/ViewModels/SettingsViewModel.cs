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

        public VmDevicePing vmDevicePingTemp = new VmDevicePing(new DevicePing());
        public VmDevicePing VmDevicePingTemp
        {
            get
            {
                return vmDevicePingTemp;
            }
            set
            {
                vmDevicePingTemp = value;
                OnPropertyChanged(nameof(VmDevicePingTemp));
            }
        }

        public VmDevicesGroup vmDeviceGroupTemp = new VmDevicesGroup(new DevicesGroup());
        public VmDevicesGroup VmDeviceGroupTemp
        {
            get
            {
                return vmDeviceGroupTemp;
            }
            set
            {
                vmDeviceGroupTemp = value;
                OnPropertyChanged(nameof(VmDeviceGroupTemp));
            }
        }

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
                Color = VmDeviceGroupTemp.Color,
                Type = VmDeviceGroupTemp.Type,
                UserSelectionID = ListenVm.Instance.UserSelectionsVm.EntityID,
            };

            try
            {
                db.DevicesGroups.Add(newDevicesGroup);
                await db.SaveChangesAsync();
                ListenVm.ReloadFromDatabase(VmDeviceGroupTemp);
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
