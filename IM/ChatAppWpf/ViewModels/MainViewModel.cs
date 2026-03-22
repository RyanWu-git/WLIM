using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChatAppWpf.Models;
using ChatAppWpf.Services;

namespace ChatAppWpf.ViewModels;

public sealed class MainViewModel : ObservableObject
{
  private readonly AuthSessionStore _sessionStore;
  private ChatSession? _selectedSession;
  private string _newMessage = string.Empty;
  private string _title = "聊天";

  public MainViewModel(AuthSessionStore sessionStore)
  {
    _sessionStore = sessionStore;
    Sessions = new ObservableCollection<ChatSession>();
    Contacts = new ObservableCollection<Contact>();
    Messages = new ObservableCollection<Message>();

    SendCommand = new RelayCommand(SendMessage, CanSend);
    SeedDemoData();
  }

  public ObservableCollection<ChatSession> Sessions { get; }

  public ObservableCollection<Contact> Contacts { get; }

  public ObservableCollection<Message> Messages { get; }

  public ICommand SendCommand { get; }

  public string CurrentUserEmail => _sessionStore.Session?.Email ?? string.Empty;
  public string CurrentUserId => _sessionStore.Session?.UserId ?? string.Empty;

  public string Title
  {
    get => _title;
    private set => SetField(ref _title, value);
  }

  public ChatSession? SelectedSession
  {
    get => _selectedSession;
    set
    {
      if (SetField(ref _selectedSession, value))
      {
        LoadDemoMessagesForSession(value);
        Title = value?.Name ?? "聊天";
        ((RelayCommand)SendCommand).RaiseCanExecuteChanged();
      }
    }
  }

  public string NewMessage
  {
    get => _newMessage;
    set
    {
      if (SetField(ref _newMessage, value))
      {
        ((RelayCommand)SendCommand).RaiseCanExecuteChanged();
      }
    }
  }

  private bool CanSend()
  {
    return SelectedSession is not null && !string.IsNullOrWhiteSpace(NewMessage);
  }

  private void SendMessage()
  {
    if (SelectedSession is null)
    {
      return;
    }

    var msg = new Message(
      Id: Guid.NewGuid().ToString("N"),
      SessionId: SelectedSession.Id,
      SenderId: _sessionStore.Session?.UserId ?? "me",
      Content: NewMessage.Trim(),
      CreatedAt: DateTimeOffset.Now
    );

    Messages.Add(msg);
    NewMessage = string.Empty;
  }

  private void SeedDemoData()
  {
    Sessions.Add(new ChatSession("demo-1", "private", "示例私聊", DateTimeOffset.Now));
    Sessions.Add(new ChatSession("demo-2", "group", "示例群聊", DateTimeOffset.Now.AddMinutes(-10)));

    Contacts.Add(new Contact("c-1", "张三", "friend"));
    Contacts.Add(new Contact("g-1", "开发群", "group"));

    SelectedSession = Sessions.FirstOrDefault();
  }

  private void LoadDemoMessagesForSession(ChatSession? session)
  {
    Messages.Clear();
    if (session is null)
    {
      return;
    }

    Messages.Add(new Message(Guid.NewGuid().ToString("N"), session.Id, "other", "你好，这是一条示例消息。", DateTimeOffset.Now.AddMinutes(-2)));
    Messages.Add(new Message(Guid.NewGuid().ToString("N"), session.Id, _sessionStore.Session?.UserId ?? "me", "收到。", DateTimeOffset.Now.AddMinutes(-1)));
  }
}
