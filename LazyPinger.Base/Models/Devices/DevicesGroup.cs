using System.ComponentModel.DataAnnotations;

namespace LazyPinger.Base.Models.Devices
{
    public class DevicesGroup : IValidatableObject
    {
        [Key]
        public int ID { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public ICollection<DevicePing> DevicePings { get; set; } = new List<DevicePing>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(this.Type, new ValidationContext(this, null, null) { MemberName = nameof(Type)}, results);
            Validator.TryValidateProperty(this.Color, new ValidationContext(this, null, null) { MemberName = nameof(Color) }, results);

            if (Type == string.Empty || Color == string.Empty)
                results.Add(new ValidationResult("Fields cannot be empty.."));

            return results;
        }
    }
}
