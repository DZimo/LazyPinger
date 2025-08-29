using LazyPinger.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LazyPinger.Base.Models.User
{
    public class UserPreference : LazyPingerEntity
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }

        public UserSelection UserSelection { get; set; }

    }
}
