using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatAppWpf.ViewModels;

public sealed class AsyncRelayCommand : ICommand
{
  private readonly Func<Task> _executeAsync;
  private readonly Func<bool>? _canExecute;
  private bool _isExecuting;

  public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
  {
    _executeAsync = executeAsync;
    _canExecute = canExecute;
  }

  public event EventHandler? CanExecuteChanged;

  public bool CanExecute(object? parameter)
  {
    if (_isExecuting)
    {
      return false;
    }

    return _canExecute?.Invoke() ?? true;
  }

  public async void Execute(object? parameter)
  {
    _isExecuting = true;
    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    try
    {
      await _executeAsync();
    }
    finally
    {
      _isExecuting = false;
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }

  public void RaiseCanExecuteChanged()
  {
    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
  }
}
