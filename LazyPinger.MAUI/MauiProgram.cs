using LazyPinger.Base.IServices;
using LazyPinger.Base.Services;
using LazyPinger.Core.Services;
using LazyPingerMAUI.ViewModels;
using LazyPingerMAUI.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace LazyPingerMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .RegisterAppServices()
                .RegisterViewModels()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("NotoSans-Regular.ttf", "NotoSans");
                    fonts.AddFont("NotoSans-Semibold.ttf", "NotoSansSemiBold");
                    fonts.AddFont("NotoSans-Medium.ttf", "NotoSansMedium");
                    fonts.AddFont("NotoSans-Light.ttf", "NotoSansLight");
                    fonts.AddFont("NotoSans-Bold.ttf", "NotoSansBold");
                    fonts.AddFont("NotoSans-Black.ttf", "NotoSansBlack");
                    fonts.AddFont("NotoSans-Italic.ttf", "NotoSansItalic");
                    fonts.AddFont("NotoSans-Bolditalic.ttf", "NotoSansBoldItalic");
                    fonts.AddFont("NotoSans_Condensed-Black.ttf", "NotoSansCondensedBlack");
                    fonts.AddFont("NotoSans_Condensed-Bold.ttf", "NotoSansCondensedBold");
                    fonts.AddFont("NotoSans_Condensed-BoldItalic.ttf", "NotoSansCondensedBoldItalic");
                    fonts.AddFont("NotoSans_Condensed-BlackItalic.ttf", "NotoSansCondensedBlackItalic");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder mauiAppBuilder)
        {
            mauiAppBuilder.Services.AddSingleton<INetworkService, NetworkService>();
            mauiAppBuilder.Services.AddSingleton<IPingerService, PingerService>();
            mauiAppBuilder.Services.AddSingleton<IArpDetectorService, ArpService>();
            mauiAppBuilder.Services.AddTransient<IImageService<ImageSource>, ImageService>();

            return mauiAppBuilder;
        }

        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
        {
            mauiAppBuilder.Services.AddSingleton<MainViewModel>();
            mauiAppBuilder.Services.AddTransient<SettingsViewModel>();

            mauiAppBuilder.Services.AddTransient<MainPage>();
            mauiAppBuilder.Services.AddTransient<SettingsPage>();
            return mauiAppBuilder;
        }
    }
}
