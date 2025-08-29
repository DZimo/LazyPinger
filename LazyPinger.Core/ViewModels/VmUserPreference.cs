using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.User;

namespace LazyPinger.Core.ViewModels
{
    public partial class VmUserPreference : LazyPingerEntityVm<LazyPingerDbContext, UserPreference>
    {
        public VmUserPreference(UserPreference dbEntity) : base(dbEntity)
        {
            this.EntityTable = db => db.UserPreferences;
            this.EntityID = dbEntity.ID;

            Name = Entity.Name;
        }

        [ObservableProperty]
        public string name;

        partial void OnNameChanged(string value) =>
            this.Entity.Name = value;
    }
}
