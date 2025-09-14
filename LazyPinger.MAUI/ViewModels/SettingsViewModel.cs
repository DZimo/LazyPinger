using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Core.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;

namespace LazyPingerMAUI.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        public MainViewModel MainVm { get; set; }

        private IPopupService popupService { get; set; }

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

        public SettingsViewModel(INetworkService networkService, MainViewModel mainViewModel, IPopupService popupService)
        {
            MainVm = mainViewModel;
            this.popupService = popupService;
        }

        [RelayCommand]
        public async Task CreateNewDevice()
        {
            if (ListenVm.Instance.UserSelectionsVm is null)
                return;

            var db = ListenVm.Instance.dbContext;
            var newDevice = new DevicePing()
            {
                Name = VmDevicePingTemp.Name,
                DevicesGroup = VmDevicePingTemp.Group.Entity,
                Image = [0, 2],
                IP = VmDevicePingTemp.Ip,
                UserSelectionID = ListenVm.Instance.UserSelectionsVm.EntityID,
                SubnetRangeMax = VmDevicePingTemp.MaxRange,
                SubnetRangeMin = VmDevicePingTemp.MinRange,
                IsIpBased = VmDevicePingTemp.Ip is not null ? true : false,
            };

            try
            {
                db.DevicePings.Add(newDevice);
                await db.SaveChangesAsync();
                ListenVm.ReloadFromDatabase(newDevice);
                VmDevicePingTemp = new(new DevicePing());

                var currentPage = Application.Current?.MainPage;

                if (currentPage != null)
                    await currentPage.DisplayAlert("Successful", "Successfully saved the new device.", "OK");

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
                VmDeviceGroupTemp = new(new DevicesGroup());

                var currentPage = Application.Current?.MainPage;

                if (currentPage != null)
                    await currentPage.DisplayAlert("Successful", "Successfully saved the new device group.", "OK");

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
