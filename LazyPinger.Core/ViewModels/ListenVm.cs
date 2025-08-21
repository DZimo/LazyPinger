﻿using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.Devices;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace LazyPinger.Core.ViewModels
{
    public class ListenVm : ObservableObject
    {
        private static ListenVm? instance;
        public static ListenVm Instance => ListenVm.instance ??= new ListenVm();

        public LazyPingerDbContext dbContext { get; set; } = new();

        private ListenVm() { }

        public ObservableCollection<DevicesGroup> devicesGroup;
        public ObservableCollection<DevicesGroup> DevicesGroup
        {
            get
            {
                return devicesGroup;
            }
            set
            {
                devicesGroup = value;
                OnPropertyChanged(nameof(DevicesGroup));
                OnPropertyChanged(nameof(DevicesPing));
            }
        }

        private ObservableCollection<DevicePing> devicesPing;
        public ObservableCollection<DevicePing> DevicesPing
        {
            get
            {
                return devicesPing;
            }
            set
            {
                devicesPing = value;
                OnPropertyChanged(nameof(DevicesPing));
                OnPropertyChanged(nameof(DevicesGroup));
            }
        }

        private ObservableCollection<VmDevicesGroup> devicesGroupVm;
        public ObservableCollection<VmDevicesGroup> DevicesGroupVm
        {
            get
            {
                return devicesGroupVm;
            }
            set
            {
                devicesGroupVm = value;
                OnPropertyChanged(nameof(DevicesPing));
                OnPropertyChanged(nameof(DevicesGroup));
                OnPropertyChanged(nameof(DevicesGroupVm));
            }
        }

        private VmUserSelection? userSelectionsVm { get; set; }
        public VmUserSelection? UserSelectionsVm
        {
            get 
            {
                return userSelectionsVm;
            }
            set
            {
                userSelectionsVm = value;
                OnPropertyChanged(nameof(UserSelectionsVm));
                OnPropertyChanged(nameof(DevicesPing));

                if (value is not null && value.IsAutoRunDisabled)
                    return;

            }
        }

        public Action GetDevicesGroup()
        {
            return () =>
            {
                try
                {
                    var res = dbContext.DevicesGroups.Include(o => o.DevicePings).ToList();
                    Instance.DevicesGroup = new ObservableCollection<DevicesGroup>(res);
                    Instance.DevicesGroupVm = new ObservableCollection<VmDevicesGroup>(res.Select(o => new VmDevicesGroup(o)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load DevicesGroup from Database: {ex.Message}");
                }
            };
        }

        public Action GetDevicesGroupVm()
        {
            return () =>
            {
                try
                {
                    var res = dbContext.DevicesGroups.Include(o => o.DevicePings).ToList().Select((o) => new VmDevicesGroup(o)
                    {
                        Color = o.Color,
                        Type = o.Type
                    }).ToList();

                    DevicesGroupVm = new ObservableCollection<VmDevicesGroup>(res);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load DevicesGroup from Database: {ex.Message}");
                }
            };
        }

        public Action GetDevicePing()
        {
            return () =>
            {
                try
                {
                    var res = dbContext.DevicePings.Include(o => o.DevicesGroup).ToList();
                    DevicesPing = new ObservableCollection<DevicePing>(res);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load DevicePing from Database: {ex.Message}");
                }
            };
        }

        public Action GetUserSelectionVm()
        {
            return async ()  =>
            {
                try
                {
                    var res = await dbContext.UserSelections.FirstAsync();
                    UserSelectionsVm = new VmUserSelection(res);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load UserSelections from Database: {ex.Message}");
                }
            };
        }
        public static void LoadAll()
        {
            List<Action> action = [ Instance.GetUserSelectionVm(), Instance.GetDevicesGroup(), Instance.GetDevicesGroupVm(), Instance.GetDevicePing() ];

            action.ToList().ForEach( o => o.Invoke());
        }

        public static void ReloadFromDatabase(object type)
        {
            var action = type switch
            {
                VmUserSelection vmUserSelection => Instance.GetUserSelectionVm(),
                DevicesGroup devicesGroup => Instance.GetDevicesGroup(),
                VmDevicesGroup devicePing => Instance.GetDevicesGroupVm(),
                DevicePing devicePing => Instance.GetDevicePing(),
            };

            action.Invoke();
        }
    }
}
