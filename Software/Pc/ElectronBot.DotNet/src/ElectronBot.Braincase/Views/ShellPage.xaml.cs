using ElectronBot.Braincase.Contracts.Services;
using ElectronBot.Braincase.Helpers;
using ElectronBot.Braincase.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Verdure.NotificationArea;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.System;
using Windows.UI.Popups;

namespace ElectronBot.Braincase.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    // 属性：通知区域图标
    public NotificationAreaIcon NotificationAreaIcon { get; set; } = new NotificationAreaIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/pig.ico"), "AppDisplayName".GetLocalized());
    public ShellViewModel ViewModel
    {
        get;
    }

    // 构造函数：初始化 ViewModel 并设置标题栏
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        // 设置导航服务的 Frame 和初始化导航视图服务
        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        // TODO: 通过更新 /Assets/WindowIcon.ico 来设置标题栏图标。
        // 为了支持完整的窗口主题和 Mica 效果，需要自定义标题栏。
        // 参考：https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
        InitializeNotificationAreaIcon();
        App.RootFrame = NavigationFrame;
        App.MainWindow.Closed += MainWindow_Closed;
    }

    // 处理窗口关闭事件，释放通知区域图标资源
    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        NotificationAreaIcon.Dispose();
    }

    // 处理加载事件，更新标题栏并初始化键盘快捷键
    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
        ViewModel.Initialize();
        await ElectronBotHelper.Instance.InitAsync();

        //await Bot3DHelper.Instance.LoadModelFileAsync();

        //await RegisterTaskAysnc();
    }

    // 初始化通知区域图标和菜单项
    private void InitializeNotificationAreaIcon()
    {
        NotificationAreaIcon.InitializeNotificationAreaMenu();
        NotificationAreaIcon.AddMenuItemText(1, "NotificationAreaIconMenuItemText".GetLocalized());
        //NotificationAreaIcon.AddMenuItemText(2, "设置");
        NotificationAreaIcon.AddMenuItemSeperator();
        NotificationAreaIcon.AddMenuItemText(3, "NotificationAreaIconExitText".GetLocalized());

        // 双击通知区域图标时执行的操作
        NotificationAreaIcon.DoubleClick = () =>
        {
            DispatcherQueue.TryEnqueue(() => { ViewModel.ShowOrHideWindowCommand.Execute(null); });
        };
        // 右键点击通知区域图标时显示上下文菜单
        NotificationAreaIcon.RightClick = () =>
        {
            DispatcherQueue.TryEnqueue(() => { NotificationAreaIcon.ShowContextMenu(); });
        };
        // 菜单项点击时执行的操作
        NotificationAreaIcon.MenuCommand = (menuid) =>
        {
            switch (menuid)
            {
                case 1:
                    {
                        DispatcherQueue.TryEnqueue(() => { ViewModel.ShowOrHideWindowCommand.Execute(null); });
                        break;
                    }
                case 2:
                    {
                        DispatcherQueue.TryEnqueue(() => { ViewModel.SettingsCommand.Execute(null); });
                        break;
                    }
                case 3:
                    {
                        DispatcherQueue.TryEnqueue(() => { ViewModel.ExitCommand.Execute(null); });
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        };
    }

    // 处理窗口激活事件，更新标题栏文本颜色
    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    // 处理导航视图显示模式变化事件，调整标题栏的边距
    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    // 构建键盘快捷键
    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }


    // 处理键盘快捷键事件，调用导航服务的返回方法
    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    // 异步注册后台任务的方法
    async Task RegisterTaskAysnc()
    {

        var taskRegistered = false;
        var exampleTaskName = "EbToastBgTask";
        taskRegistered = BackgroundTaskRegistration.AllTasks.Any(x => x.Value.Name == exampleTaskName);


        if (!taskRegistered)
        {
            var access = await BackgroundExecutionManager.RequestAccessAsync();
            if (access == BackgroundAccessStatus.DeniedBySystemPolicy)
            {
                await new MessageDialog("后台任务已经被禁止了").ShowAsync();
            }
            else
            {
                var builder = new BackgroundTaskBuilder
                {
                    Name = "EbToastBgTask",
                    TaskEntryPoint = "ElectronBot.Braincase.BgTaskComponent.ToastBgTask"
                };
                builder.SetTrigger(new TimeTrigger(15, false));

                var task = builder.Register();
            }

        }
        else
        {
            var cur = BackgroundTaskRegistration.AllTasks.FirstOrDefault(x => x.Value.Name == exampleTaskName);
            BackgroundTaskRegistration task = (BackgroundTaskRegistration)(cur.Value);
            //    task.Completed += task_Completed;
        }
    }
}
