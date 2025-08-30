using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.Devices;

namespace LazyPinger.Core.ViewModels
{
    public partial class VmDevicePing : LazyPingerEntityVm<LazyPingerDbContext, DevicePing>
    {
        public VmDevicePing(DevicePing dbEntity) : base(dbEntity)
        {
            this.EntityTable = db => db.DevicePings;
            this.EntityID = dbEntity.ID;

            Name = Entity.Name;
            Image = Entity.Image;
            Group = Entity.DevicesGroup;
            Ip = Entity?.IP;
        }

        [ObservableProperty]
        public DevicesGroup group;

        [ObservableProperty]
        public byte[] image;

        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public string ip;

        public string type;

        [ObservableProperty]
        public bool canCreateDevicePing;

        partial void OnGroupChanged(DevicesGroup value) =>
            this.Entity.DevicesGroup = value;

        partial void OnNameChanged(string value) =>
            this.Entity.Name = value;

        partial void OnImageChanged(byte[] value) =>
            this.Entity.Image = value;

        partial void OnIpChanged(string value) =>
            this.Entity.IP = value;


    }
}
