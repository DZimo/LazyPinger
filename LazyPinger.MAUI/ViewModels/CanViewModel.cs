using CommunityToolkit.Mvvm.ComponentModel;
using LazyPinger.Base.Entities;
using LazyPinger.Base.Models.Devices;
using LazyPinger.Base.Models.Network;
using System.Collections.ObjectModel;

namespace LazyPinger.MAUI.ViewModels
{
    public partial class CanViewModel : ViewModelBase
    {

        public CanViewModel() { }

        [ObservableProperty]
        private string selectedCanType = "Receiver";

        public ObservableCollection<CanFrame> CanDataVm { get; set; } = new();

        public ViewModelBase CanView { get; set; }

        partial void OnSelectedCanTypeChanged(string? oldValue, string newValue)
        {
            if (oldValue == newValue)  
                return;


        }

    }
}
