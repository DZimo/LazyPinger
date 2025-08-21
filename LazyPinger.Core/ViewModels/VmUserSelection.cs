using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models;

namespace LazyPinger.Core.ViewModels
{
    public partial class VmUserSelection : LazyPingerEntityVm<LazyPingerDbContext, UserSelection>
    {
        public VmUserSelection(UserSelection dbEntity) : base(dbEntity)
        {
            this.EntityTable = db => db.UserSelections;
            this.EntityID = dbEntity.ID;

            IsAutoRunDisabled = Entity.AutoRun;
            IsFastPingDisabled = Entity.FastPing;
            IsAutoRestartDisabled = Entity.AutoRestart;
        }

        [ObservableProperty]
        public bool isAutoRunDisabled;

        [ObservableProperty]
        public bool isFastPingDisabled;

        [ObservableProperty]
        public bool isAutoRestartDisabled;

        partial void OnIsAutoRunDisabledChanged(bool value) =>
            this.Entity.AutoRun = value;
        partial void OnIsFastPingDisabledChanged(bool value) =>
            this.Entity.FastPing = value;
        partial void OnIsAutoRestartDisabledChanged(bool value)
        {
            this.Entity.AutoRestart = value;

            if (value is false)
                return;
        }
    }
}
