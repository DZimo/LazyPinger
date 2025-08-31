using LazyPinger.Base.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace LazyPingerMAUI
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider services)
        {
            InitializeComponent();

            MainPage = new AppShell(services);
            Services = services;

            using var context = new LazyPingerDbContext();

         
        }
    }
}
