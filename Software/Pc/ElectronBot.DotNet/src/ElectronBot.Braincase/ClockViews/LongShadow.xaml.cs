using System.Numerics;
using ElectronBot.Braincase.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ElectronBot.Braincase.ClockViews;
public sealed partial class LongShadow : UserControl
{
    public ClockViewModel ViewModel
    {
        get;
    }
    public LongShadow()
    {
        this.InitializeComponent();

        ViewModel = App.GetService<ClockViewModel>();

        // 创建多个阴影效果的调用
        MakeLongShadow(188, 0.3f, InWorkCountDown, InworkBackground, Color.FromArgb(255, 250, 110, 93));
        MakeLongShadow(188, 0.3f, InWorkCountDownSecond, InworkSecondBackground, Color.FromArgb(255, 250, 110, 93));

        MakeLongShadow(188, 0.3f, CustomTitleTb, CustomTitleBackground, Color.FromArgb(255, 250, 110, 93));
        MakeLongShadow(188, 0.3f, Day, DayBackground, Color.FromArgb(255, 250, 110, 93));

        // 设置 `FlipSide` 的初始状态为未翻转。
        FlipSide.IsFlipped = false; 
    }

    private void MakeLongShadow(int depth, float opacity, TextBlock textElement, FrameworkElement shadowElement, Color color)
    {
        var textVisual = ElementCompositionPreview.GetElementVisual(textElement);  // 获取文本元素的视觉元素。
        var compositor = textVisual.Compositor;  // 获取合成器（Compositor），用于创建视觉效果。
        var containerVisual = compositor.CreateContainerVisual();  // 创建一个容器视觉，用于存放所有阴影层。
        var mask = textElement.GetAlphaMask();  // 获取文本的透明度蒙版，用于生成阴影。
        Vector3 background = new Vector3(color.R, color.G, color.B);  // 将颜色转换为三维向量。

        for (int i = 0; i < depth; i++)
        {
            var maskBrush = compositor.CreateMaskBrush();  // 创建一个蒙版画刷。

            // 计算阴影颜色的逐渐变化。
            var shadowColor = background - ((background - new Vector3(0, 0, 0)) * opacity);
            shadowColor = Vector3.Max(Vector3.Zero, shadowColor);
            shadowColor += (background - shadowColor) * i / depth;

            maskBrush.Mask = mask;  // 设置蒙版。
            maskBrush.Source = compositor.CreateColorBrush(Color.FromArgb(255, (byte)shadowColor.X, (byte)shadowColor.Y, (byte)shadowColor.Z));  // 使用计算后的颜色创建画刷。

            var visual = compositor.CreateSpriteVisual();  // 创建一个新的视觉元素。
            visual.Brush = maskBrush;  // 为视觉元素设置画刷。
            visual.Offset = new Vector3(i + 1, i + 1, 0);  // 设置偏移量，使得每一层阴影略微移动，形成深度效果。

            var bindSizeAnimation = compositor.CreateExpressionAnimation("textVisual.Size");  // 创建一个绑定大小的动画，使阴影的大小与文本大小同步。
            bindSizeAnimation.SetReferenceParameter("textVisual", textVisual);  // 设置引用参数为文本的视觉元素。
            visual.StartAnimation("Size", bindSizeAnimation);  // 开始动画。

            containerVisual.Children.InsertAtBottom(visual);  // 将每一层的阴影添加到容器视觉的底部。
        }

        ElementCompositionPreview.SetElementChildVisual(shadowElement, containerVisual);  // 将生成的阴影视觉设置为背景元素的子视觉。
    }

}
