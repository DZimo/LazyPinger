using LazyPinger.MAUI.ViewModels;

namespace LazyPinger.MAUI.Views.CAN;

public partial class CanPage : ContentPage
{
    public CanPage(CanViewModel canVm)
    {
        InitializeComponent();
        this.BindingContext = canVm;
    }
}