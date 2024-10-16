using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronBot.Braincase.Services;
using Verdure.ElectronBot.Core.Contracts.Services;
using Verdure.ElectronBot.Core.Services;
using Microsoft.Graph;
using Controls.CompactOverlay;
using Microsoft.UI.Windowing;

namespace ElectronBot.Braincase.ViewModels;

// 定义一个 ViewModel 类，用于管理待办事项的数据和行为。
public partial class TodoViewModel : ObservableRecipient
{
    private ICommand _loadedCommand; // 用于页面加载时的命令。
    // 获取加载命令，懒加载，如果为空则创建一个新的 RelayCommand，并绑定到 OnLoaded 方法。
    public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(OnLoaded);

    private readonly IMicrosoftGraphService _microsoftGraphService; // Microsoft Graph 服务的接口实例，用于获取任务列表。
    private readonly IdentityService _identityService; // 身份验证服务，用于管理用户登录状态。

    private bool _isLogin = false; // 标记用户是否已经登录。

    private ObservableCollection<TodoTaskList> _todoTaskLists = new(); // 保存待办事项列表的可观察集合。

    // 构造函数，接受 Microsoft Graph 服务和身份验证服务的实例。
    public TodoViewModel(IMicrosoftGraphService microsoftGraphService,
        IdentityService identityService)
    {
        _microsoftGraphService = microsoftGraphService;
        _identityService = identityService;
    }

    // 公开的 TodoTaskLists 属性，支持数据绑定。
    public ObservableCollection<TodoTaskList> TodoTaskLists
    {
        get => _todoTaskLists; // 返回待办事项列表。
        set => SetProperty(ref _todoTaskLists, value); // 设置待办事项列表，并触发属性更改通知。
    }

    // 异步方法，当页面加载时调用。
    private async void OnLoaded()
    {
        // 检查用户是否已登录。
        if (_identityService.IsLoggedIn())
        {
            await _microsoftGraphService.PrepareGraphAsync(); // 准备 Microsoft Graph 服务。

            var todos = await _microsoftGraphService.GetTodoTaskListAsync(); // 获取待办事项列表。

            // 将获取到的任务列表赋值给 TodoTaskLists。
            TodoTaskLists = new ObservableCollection<TodoTaskList>(todos);
        }
        else
        {
            // 如果用户未登录，订阅登录事件。
            _identityService.LoggedIn += IdentityService_LoggedIn;
            await _identityService.LoginAsync(); // 发起登录。
        }
    }

    // 异步方法，当用户成功登录后触发。
    private async void IdentityService_LoggedIn(object? sender, EventArgs e)
    {
        _isLogin = true; // 设置登录状态为 true。

        var todos = await _microsoftGraphService.GetTodoTaskListAsync(); // 获取待办事项列表。

        // 将获取到的任务列表赋值给 TodoTaskLists。
        TodoTaskLists = new ObservableCollection<TodoTaskList>(todos);
    }

    // 使用 CommunityToolkit 的特性定义的命令，切换到紧凑视图模式。
    [RelayCommand]
    public void CompactOverlay()
    {
        // 创建一个新的紧凑视图窗口实例。
        WindowEx compactOverlay = new CompactOverlayWindow();

        // 设置紧凑视图窗口的内容为默认的紧凑视图页面。
        compactOverlay.Content = new DefaultCompactOverlayPage();

        var appWindow = compactOverlay.AppWindow; // 获取窗口实例。

        appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay); // 设置窗口为紧凑模式。

        appWindow.Show(); // 显示紧凑视图窗口。

        App.MainWindow.Hide(); // 隐藏主窗口。
    }
}
