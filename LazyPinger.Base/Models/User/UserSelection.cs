using LazyPinger.Base.Entities;
using LazyPinger.Base.Models.Devices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LazyPinger.Base.Models.User
{
    public class UserSelection : LazyPingerEntity
    {
        [Key]
        public int ID { get; set; }

        public bool AutoRun { get; set; }

        public bool FastPing { get; set; }

        public bool FastnessLevel { get; set; }

        public bool AutoRestart { get; set; }

        public int AutoRestartTime { get; set; }

        public ICollection<DevicePing>? DevicesPing { get; set; }

        public int UserPreferenceID { get; set; }

        [ForeignKey(nameof(UserPreferenceID))]
        public UserPreference UserPreference { get; set; }

    }
}
