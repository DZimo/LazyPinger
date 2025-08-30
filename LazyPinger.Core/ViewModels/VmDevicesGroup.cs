using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.Devices;
using System.ComponentModel.DataAnnotations;

namespace LazyPinger.Core.ViewModels
{
    public partial class VmDevicesGroup : LazyPingerEntityVm<LazyPingerDbContext, DevicesGroup>, IValidatableObject
    {
        public VmDevicesGroup(DevicesGroup dbEntity) : base(dbEntity)
        {
            this.EntityTable = db => db.DevicesGroups;
            this.EntityID = dbEntity.ID;

            this.Type = Entity.Type;
            this.Color = Entity.Color;
        }

        [ObservableProperty]
        public string color;

        [ObservableProperty]
        public string type;

        [ObservableProperty]
        public bool canCreateDeviceGroup;

        partial void OnTypeChanged(string value)
        {
            var res = Validate(null);

            if (res.Count() > 0)
            {
                CanCreateDeviceGroup = false;
                return;
            }

            CanCreateDeviceGroup = true;
        }

        partial void OnColorChanged(string value)
        {
            var res = Validate(null);

            if (res.Count() > 0)
            {
                CanCreateDeviceGroup = false;
                return;
            }

            CanCreateDeviceGroup = true;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext? validationContext)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(this.Type, new ValidationContext(this, null, null) { MemberName = nameof(Type) }, results);
            Validator.TryValidateProperty(this.Color, new ValidationContext(this, null, null) { MemberName = nameof(Color) }, results);

            if (Type == string.Empty || Color == string.Empty)
                results.Add(new ValidationResult("Fields cannot be empty.."));

            return results;
        }
    }
}
