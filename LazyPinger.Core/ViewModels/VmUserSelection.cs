using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models;
using LazyPinger.Base.Models.User;

namespace LazyPinger.Core.ViewModels
{
    public partial class VmUserSelection : LazyPingerEntityVm<LazyPingerDbContext, UserSelection>
    {
        public VmUserSelection(UserSelection dbEntity) : base(dbEntity)
        {
            this.EntityTable = db => db.UserSelections;
            this.EntityID = dbEntity.ID;

            IsAutoRunEnabled = Entity.AutoRun;
            IsFastPingEnabled = Entity.FastPing;
            IsAutoRestartEnabled = Entity.AutoRestart;
            AutoRestartTime = Entity.AutoRestartTime;
        }

        [ObservableProperty]
        public bool isAutoRunEnabled;

        [ObservableProperty]
        public bool isFastPingEnabled;

        [ObservableProperty]
        public bool isAutoRestartEnabled;

        [ObservableProperty]
        public int autoRestartTime;

        partial void OnIsAutoRunEnabledChanged(bool value) =>
            this.Entity.AutoRun = value;
        partial void OnIsFastPingEnabledChanged(bool value) =>
            this.Entity.FastPing = value;
        partial void OnAutoRestartTimeChanged(int value) =>
            this.Entity.AutoRestartTime = value;

        partial void OnIsAutoRestartEnabledChanged(bool value) =>
            this.Entity.AutoRestart = value;
    }
}
