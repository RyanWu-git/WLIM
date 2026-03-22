using System.Windows;
using System.Windows.Input;
using ChatAppWpf.ViewModels;

namespace ChatAppWpf.Views;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    MessageBox.PreviewKeyDown += OnMessageBoxPreviewKeyDown;
  }

  private void OnMessageBoxPreviewKeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
    {
      if (DataContext is MainViewModel vm && vm.SendCommand.CanExecute(null))
      {
        vm.SendCommand.Execute(null);
        e.Handled = true;
      }
    }
  }
}
