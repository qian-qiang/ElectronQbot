using System;
using System.Numerics;
using CommunityToolkit.WinUI;
using ElectronBot.Braincase.ViewModels;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Windows.UI;
using Windows.Foundation;

// 定义命名空间 ElectronBot.Braincase.ClockViews
namespace ElectronBot.Braincase.ClockViews;

// 定义 CustomClockView 类，它继承自 UserControl
public sealed partial class CustomClockView : UserControl
{
    // 默认 DPI 值
    const float defaultDpi = 96;

    // 用于存储模糊后的图像
    CanvasRenderTarget glassSurface;

    // 背景图像
    CanvasBitmap imgbackground;

    // 高斯模糊效果
    GaussianBlurEffect blurEffect;

    // 用于雨滴效果的对象
    RainyDay rainday;

    // 缩放因子和图像的宽度、高度、位置
    float scalefactor;
    float imgW;
    float imgH;
    float imgX;
    float imgY;

    // 视图模型，用于绑定数据
    public ClockViewModel ViewModel
    {
        get;
    }

    // 构造函数，初始化组件并获取 ClockViewModel 实例
    public CustomClockView()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<ClockViewModel>();
    }

    // 创建 Canvas 资源时调用的方法
    private async void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        await PrepareRaindayAsync(sender);
    }

    // 异步准备雨滴效果的方法
    private async Task PrepareRaindayAsync(CanvasControl sender)
    {
        // 获取默认背景图像路径
        var imgPath = Path.Combine(AppContext.BaseDirectory, $"Assets/Images/CustomViewDefault.jpg");

        // 默认模糊量
        var blurAmount = 4.0f;

        // 如果 ViewModel 的设置存在，则根据设置进行自定义
        if (ViewModel.BotSetting != null)
        {
            // 如果自定义图片路径不为空，则使用该路径
            if (!string.IsNullOrEmpty(ViewModel.BotSetting.CustomViewPicturePath))
            {
                imgPath = ViewModel.BotSetting.CustomViewPicturePath;
            }

            // 使用设置中的高斯模糊值
            blurAmount = ViewModel.BotSetting.GaussianBlurValue;

            // 如果自定义内容不可见，则隐藏 Pomodoro 面板
            if (!ViewModel.BotSetting.CustomViewContentIsVisibility)
            {
                PomodoroPanel.Visibility = Visibility.Collapsed;
            }
        }

        // 加载背景图像
        imgbackground = await CanvasBitmap.LoadAsync(sender, imgPath);

        // 创建高斯模糊效果并设置模糊参数
        blurEffect = new GaussianBlurEffect()
        {
            Source = imgbackground,
            BlurAmount = blurAmount,
            BorderMode = EffectBorderMode.Soft
        };

        // 计算图像缩放因子，并调整图像尺寸和位置
        scalefactor = (float)Math.Min(sender.Size.Width / imgbackground.Size.Width, sender.Size.Height / imgbackground.Size.Height);
        imgW = (float)imgbackground.Size.Width * scalefactor;
        imgH = (float)imgbackground.Size.Height * scalefactor;
        imgX = (float)(sender.Size.Width - imgW) / 2;
        imgY = (float)(sender.Size.Height - imgH) / 2;

        // 创建模糊后的图像表面
        glassSurface = new CanvasRenderTarget(sender, imgW, imgH, defaultDpi);

        //// 配置雨滴效果的参数
        //List<List<float>> pesets;

        //// 初始化 RainyDay 对象，并设置缩放因子和重力角度
        //rainday = new RainyDay(sender, imgW, imgH, imgbackground)
        //{
        //    ImgSclaeFactor = scalefactor,
        //    GravityAngle = (float)Math.PI / 2 // 垂直方向的重力效果
        //};

        //// 预设不同大小和速度的雨滴参数
        //pesets = new List<List<float>>()
        //{
        //    new List<float> { 3, 3, 0.88f },
        //    new List<float> { 5, 5, 0.9f },
        //    new List<float> { 6, 2, 1 }
        //};

        //// 使用预设参数生成雨滴效果
        //rainday.Rain(pesets, 100);
    }

    // 在 Canvas 上绘制内容时调用的方法
    private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (imgbackground != null)
        {
            // 绘制带有模糊效果的背景图像
            args.DrawingSession.DrawImage(blurEffect, new Rect(imgX, imgY, imgW, imgH), new Rect(0, 0, imgbackground.Size.Width, imgbackground.Size.Height));

            // 绘制模糊后的图像表面
            args.DrawingSession.DrawImage(glassSurface, imgX, imgY);

            // 使用绘制会话更新雨滴效果
            //using var ds = glassSurface.CreateDrawingSession();
            //rainday.UpdateDrops(ds);
        }

        // 使 Canvas 重新绘制，以实现动画效果
        canvas.Invalidate();
    }

    // 当 UserControl 被卸载时调用的方法
    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        // 从可视化树中移除 Canvas，并释放资源
        canvas.RemoveFromVisualTree();
        canvas = null;
    }

    // 当 UserControl 被加载时调用的方法
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化演示数据，将 DataContext 设置为当前对象
        InitDemoData();
    }

    // 初始化演示数据的方法
    void InitDemoData()
    {
        this.DataContext = this;
    }
}
