using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.Devices;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace LazyPinger.Core.ViewModels
{
    public partial class VmDevicePing : LazyPingerEntityVm<LazyPingerDbContext, DevicePing>, IValidatableObject
    {
        public VmDevicePing(DevicePing dbEntity) : base(dbEntity)
        {
            this.EntityTable = db => db.DevicePings;
            this.EntityID = dbEntity.ID;

            Name = Entity.Name;
            Image = Entity?.Image;
            Ip = Entity?.IP;
            MinRange = Entity.SubnetRangeMin;
            MaxRange = Entity.SubnetRangeMax;
        }

        [ObservableProperty]
        public VmDevicesGroup group;

        [ObservableProperty]
        public byte[] image;

        [ObservableProperty]
        public string name;

        [ObservableProperty]
        public string ip;

        [ObservableProperty]
        public string minRange;

        [ObservableProperty]
        public string maxRange;

        [ObservableProperty]
        public string type;

        [ObservableProperty]
        public bool canCreateDevicePing;

        partial void OnGroupChanged(VmDevicesGroup value)
        {
            this.Entity.DevicesGroup = value.Entity;
            Validate(null);
        }

        partial void OnNameChanged(string value)
        {
            this.Entity.Name = value;
            Validate(null);
        }

        partial void OnImageChanged(byte[] value)
        {
            this.Entity.Image = value;
            Validate(null);
        }

        partial void OnIpChanged(string value)
        {
            this.Entity.IP = value;
            Validate(null);
        }

        partial void OnMinRangeChanged(string value)
        {
            this.Entity.SubnetRangeMin = value;
            Validate(null);
        }

        partial void OnMaxRangeChanged(string value)
        {
            this.Entity.SubnetRangeMax = value;
            Validate(null);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext? validationContext)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(this.Name, new ValidationContext(this, null, null) { MemberName = nameof(Name) }, results);
            Validator.TryValidateProperty(this.Image, new ValidationContext(this, null, null) { MemberName = nameof(Image) }, results);
            Validator.TryValidateProperty(this.Ip, new ValidationContext(this, null, null) { MemberName = nameof(Ip) }, results);
            Validator.TryValidateProperty(this.Type, new ValidationContext(this, null, null) { MemberName = nameof(Type) }, results);

            if (string.IsNullOrEmpty(Name) || Group == null || ( string.IsNullOrEmpty(Ip) && (string.IsNullOrEmpty(MinRange) || string.IsNullOrEmpty(MaxRange))))
            {
                CanCreateDevicePing = false;
                results.Add(new ValidationResult("Fields cannot be empty.."));
                return results;
            }

            CanCreateDevicePing = true;
            return results;
        }
    }
}
