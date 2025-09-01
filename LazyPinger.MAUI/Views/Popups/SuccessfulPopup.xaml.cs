using LazyPingerMAUI.ViewModels;

namespace LazyPinger.MAUI.Views.Popups;

public partial class SuccessfulPopup : ContentView
{
	public SuccessfulPopup(SettingsViewModel settingsViewModel)
	{
		InitializeComponent();
		BindingContext = settingsViewModel;
    }
}