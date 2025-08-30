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
            Validate(null);
        }

        partial void OnColorChanged(string value)
        {
            Validate(null);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext? validationContext)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(this.Type, new ValidationContext(this, null, null) { MemberName = nameof(Type) }, results);
            Validator.TryValidateProperty(this.Color, new ValidationContext(this, null, null) { MemberName = nameof(Color) }, results);

            if (Type == string.Empty || Color == string.Empty)
            {
                CanCreateDeviceGroup = false;
                results.Add(new ValidationResult("Fields cannot be empty.."));
                return results;
            }

            CanCreateDeviceGroup = true;
            return results;
        }
    }
}
