﻿using LazyPinger.Base.Entities;
using LazyPinger.Base.IServices;
using LazyPinger.Base.Models;
using LazyPinger.Core.Utils;
using LazyPinger.Core.ViewModels;
using LazyPingerMAUI.ViewModels;

namespace LazyPingerMAUI.Views
{
    public partial class NetworkPage : ContentPage
    {
        private INetworkService _networkService { get; set; }

        private MainViewModel _mainViewModel { get; set; }

        public NetworkPage(INetworkService networkService, MainViewModel mainVm)
        {
            InitializeComponent();

            _networkService = networkService;
            _mainViewModel = mainVm;

            this.BindingContext = _mainViewModel;

            Task.Run(async () => {
                await Task.Delay(_mainViewModel.AnimationHandler.WaitTimeToHideSplash);

                _mainViewModel.AnimationHandler = new AnimationHandler()
                {
                    IsSplashVisible = false,
                };

                await Task.Delay(_mainViewModel.AnimationHandler.WaitTimeToHideGrey);

                _mainViewModel.AnimationHandler = new AnimationHandler()
                {
                    IsGreyVisible = false,
                    IsSplashVisible = false,
                };

                await Task.Delay(_mainViewModel.AnimationHandler.WaitTimeToHideLogo);

                _mainViewModel.AnimationHandler = new AnimationHandler()
                {
                    //DevicesRowSpan = 2,
                    //DevicesRow = 0,
                    IsTopLogoVisible = false,
                    IsGreyVisible = false,
                    IsSplashVisible = false,
                };
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            try
            {
                ListenVm.Instance.dbContext.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                
            }
        }
    }

}
