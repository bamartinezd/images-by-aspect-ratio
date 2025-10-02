using Microsoft.Maui.Controls;

namespace MauiAppDbif
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
