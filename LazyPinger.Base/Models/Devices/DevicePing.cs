using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LazyPinger.Base.Models.Devices
{
    public class DevicePing : IValidatableObject
    {
        [Key]
        public int ID { get; set; }

        public int DevicesGroupID { get; set; }

        [ForeignKey(nameof(DevicesGroupID))]
        public DevicesGroup DevicesGroup { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? IP {  get; set; }

        public string? Port { get; set; }

        public string? MacAddress { get; set; }

        [Required]
        public string Type { get; set; } = "Unknown";

        [Required]
        public string Color { get; set; } = "#212121";

        [Required]
        public byte[]? Image { get; set; }

        public string? AnswerTime { get; set; } = "0ms";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(this.Name, new ValidationContext(this, null, null) { MemberName = nameof(Name) }, results);
            Validator.TryValidateProperty(this.IP, new ValidationContext(this, null, null) { MemberName = nameof(IP) }, results);

            if (Type == string.Empty || Color == string.Empty)
                results.Add(new ValidationResult("Fields cannot be empty.."));

            return results;
        }

    }
}
