using LazyPinger.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace LazyPinger.Base.Models
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

    }
}
