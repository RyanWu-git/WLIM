using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatAppWpf.Services;

namespace ChatAppWpf.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
  private readonly SupabaseAuthClient _authClient;
  private readonly AuthSessionStore _sessionStore;

  private string _email = string.Empty;
  private string _password = string.Empty;
  private string _error = string.Empty;
  private bool _isBusy;
  private readonly AsyncRelayCommand _loginCommand;

  public LoginViewModel(SupabaseAuthClient authClient, AuthSessionStore sessionStore)
  {
    _authClient = authClient;
    _sessionStore = sessionStore;
    _loginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    LoginCommand = _loginCommand;
  }

  public event EventHandler? LoginSucceeded;

  public string Email
  {
    get => _email;
    set
    {
      if (SetField(ref _email, value))
      {
        _loginCommand.RaiseCanExecuteChanged();
      }
    }
  }

  public string Password
  {
    get => _password;
    set
    {
      if (SetField(ref _password, value))
      {
        _loginCommand.RaiseCanExecuteChanged();
      }
    }
  }

  public string Error
  {
    get => _error;
    private set => SetField(ref _error, value);
  }

  public bool IsBusy
  {
    get => _isBusy;
    private set => SetField(ref _isBusy, value);
  }

  public ICommand LoginCommand { get; }

  private bool CanLogin()
  {
    return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
  }

  private async Task LoginAsync()
  {
    Error = string.Empty;
    IsBusy = true;

    try
    {
      var session = await _authClient.SignInWithPasswordAsync(Email.Trim(), Password, CancellationToken.None);
      _sessionStore.Set(session);
      LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }
    catch (Exception ex)
    {
      Error = ex.Message;
    }
    finally
    {
      IsBusy = false;
    }
  }
}
