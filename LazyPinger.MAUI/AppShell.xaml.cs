using LazyPingerMAUI.ViewModels;

namespace LazyPingerMAUI
{
    public partial class AppShell : Shell
    {
        public AppShell(IServiceProvider service)
        {
            InitializeComponent();
            this.BindingContext = service.GetServices<MainViewModel>().First();
        }
    }
}
