using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;

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
                OnPropertyChanged(nameof(DevicesPingVm));
            }
        }

        private ObservableCollection<VmDevicePing>? devicesPingVm;
        public ObservableCollection<VmDevicePing>? DevicesPingVm
        {
            get
            {
                return devicesPingVm;
            }
            set
            {
                devicesPingVm = value;
                OnPropertyChanged(nameof(DevicesPingVm));
                OnPropertyChanged(nameof(DevicesGroup));
            }
        }

        private ObservableCollection<VmDevicesGroup> devicesGroupVm = new();
        public ObservableCollection<VmDevicesGroup> DevicesGroupVm
        {
            get
            {
                return devicesGroupVm;
            }
            set
            {
                devicesGroupVm = value;
                OnPropertyChanged(nameof(DevicesPingVm));
                OnPropertyChanged(nameof(DevicesGroup));
                OnPropertyChanged(nameof(DevicesGroupVm));
            }
        }

        private VmUserSelection? userSelectionsVm;
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
                OnPropertyChanged(nameof(DevicesPingVm));

                var res = UserSelectionsVm?.Entity?.DevicesPing?.Select(p => new VmDevicePing(p));

                if (res is null)
                    return;

                DevicesPingVm = new ObservableCollection<VmDevicePing>(res);
            }
        }

        private ObservableCollection<VmUserPreference> userPreferencesVm = new();
        public ObservableCollection<VmUserPreference> UserPreferencesVm
        {
            get
            {
                return userPreferencesVm;
            }
            set
            {
                userPreferencesVm = value;
                OnPropertyChanged(nameof(UserPreferencesVm));

            }
        }

        private VmUserPreference? currentUserPreference;

        public VmUserPreference? CurrentUserPreferenceVm
        {
            get
            {
                return currentUserPreference;
            }
            set
            {
                currentUserPreference = value;
                OnPropertyChanged(nameof(CurrentUserPreferenceVm));

                GetUserSelectionVm();
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
                    //DevicesPingVm = new ObservableCollection<DevicePing>(res);
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
                    if (CurrentUserPreferenceVm is null)
                        return;

                    var res = await dbContext.UserSelections.Where(o => o.UserPreferenceID == CurrentUserPreferenceVm.EntityID).FirstAsync();
                    UserSelectionsVm = new VmUserSelection(res);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load UserSelections from Database: {ex.Message}");
                }
            };
        }

        public Action GetUserPreferences()
        {
            return async () =>
            {
                try
                {
                    var res = await dbContext.UserPreferences.Include(o => o.UserSelection).Include(s => s.UserSelection.DevicesPing).ToListAsync();
                    UserPreferencesVm = new ObservableCollection<VmUserPreference>(res.Select(p => new VmUserPreference(p)));
                    CurrentUserPreferenceVm = UserPreferencesVm?.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load UserSelections from Database: {ex.Message}");
                }
            };
        }

        public static void LoadAll()
        {
            List<Action> action = [ Instance.GetUserPreferences(), Instance.GetUserSelectionVm(), Instance.GetDevicesGroup(), Instance.GetDevicesGroupVm(), Instance.GetDevicePing() ];

            action.ToList().ForEach( o => o.Invoke());
        }

        public static void ReloadFromDatabase(object type)
        {
            var action = type switch
            {
                VmUserPreference vmUserPreference => Instance.GetUserPreferences(),
                VmUserSelection vmUserSelection => Instance.GetUserSelectionVm(),
                DevicesGroup devicesGroup => Instance.GetDevicesGroup(),
                VmDevicesGroup devicePing => Instance.GetDevicesGroupVm(),
                DevicePing devicePing => Instance.GetDevicePing(),
            };

            action.Invoke();
        }
    }
}
