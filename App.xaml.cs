using AISDisciplineDesc.Services;
using System.Configuration;
using System.Data;
using System.Windows;
using WinApp = System.Windows.Application;

namespace AISDisciplineDesc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : WinApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppState.InitializeUsbAuth();
            base.OnStartup(e);

            if (AppState.Supabase == null)
                AppState.Supabase = new SupabaseClient();

            AppState.Logger = new LoggerService(AppState.Supabase);
        }
    }

}
