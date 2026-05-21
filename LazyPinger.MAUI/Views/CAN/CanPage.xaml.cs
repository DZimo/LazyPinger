using LazyPinger.Base.IServices;
using LazyPingerMAUI.ViewModels;

namespace LazyPinger.MAUI.Views.CAN;

public partial class CanPage : ContentPage
{
	private readonly CanViewModel _viewModel;

	public CanPage(ICanService canService)
	{
		InitializeComponent();
		_viewModel = new CanViewModel(canService);
		this.BindingContext = _viewModel;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.Dispose();
	}
}
