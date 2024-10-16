using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ElectronBot.Braincase;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ElectronBot.Braincase.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ElectronBot.Braincase.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PoseRecognitionPage : Page
    {
        public PoseRecognitionViewModel ViewModel
        {
            get;
        }

        public PoseRecognitionPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<PoseRecognitionViewModel>();
        }


        private void ModelLoadCompactOverlayPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            ModelProgressRing.IsActive = true;
            ViewModel.Loaded();
            ModelProgressRing.IsActive = false;
        }

        private async void ModelLoadCompactOverlayPage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.UnLoaded();
        }
    }
}
