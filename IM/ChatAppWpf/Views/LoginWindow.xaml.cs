using System.Windows;
using ChatAppWpf.ViewModels;

namespace ChatAppWpf.Views;

public partial class LoginWindow : Window
{
  public LoginWindow()
  {
    InitializeComponent();
    PasswordBox.PasswordChanged += (_, _) =>
    {
      if (DataContext is LoginViewModel vm)
      {
        vm.Password = PasswordBox.Password;
      }
    };
  }
}
