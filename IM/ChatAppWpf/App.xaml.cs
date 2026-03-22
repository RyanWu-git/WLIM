using System.Windows;

namespace ChatAppWpf;

public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);

    var sessionStore = new Services.AuthSessionStore();
    var config = Services.AppConfig.TryLoadNearest("appsettings.json") ?? new Services.AppConfig();
    var authClient = new Services.SupabaseAuthClient(config);

    var vm = new ViewModels.LoginViewModel(authClient, sessionStore);
    var loginWindow = new Views.LoginWindow { DataContext = vm };
    vm.LoginSucceeded += (_, _) =>
    {
      var mainVm = new ViewModels.MainViewModel(sessionStore);
      var mainWindow = new Views.MainWindow { DataContext = mainVm };
      mainWindow.Show();
      loginWindow.Close();
    };
    loginWindow.Show();
  }
}
