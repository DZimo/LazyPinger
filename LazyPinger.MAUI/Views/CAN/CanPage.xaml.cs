using LazyPinger.Base.IServices;
using LazyPingerMAUI.ViewModels;

namespace LazyPinger.MAUI.Views.CAN;

public partial class CanPage : ContentPage
{
	public CanPage(ICanService canService)
	{
		InitializeComponent();
		this.BindingContext = new CanViewModel(canService);
	}
}
