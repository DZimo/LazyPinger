﻿using CommunityToolkit.Maui.Core;
using LazyPinger.Base.IServices;
using LazyPingerMAUI.ViewModels;
using CommunityToolkit.Maui;

namespace LazyPingerMAUI.Views
{
    public partial class SettingsPage : ContentPage
    {
        private INetworkService _networkService { get; set; }

        private MainViewModel _mainViewModel { get; set; }

        public SettingsPage(INetworkService networkService, MainViewModel mainViewModel, IPopupService popupService)
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel(networkService, mainViewModel, popupService);

            _networkService = networkService;
            _mainViewModel = mainViewModel;
        }
    }

}
