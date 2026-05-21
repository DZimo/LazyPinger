using LazyPingerMAUI.ViewModels;

namespace LazyPinger.MAUI.Views.CAN;

public partial class CanPage : ContentPage
{
	private readonly CanViewModel _viewModel;

	public CanPage(CanViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.Dispose();
	}
}
