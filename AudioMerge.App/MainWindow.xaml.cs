using AudioMerge.App.ViewModels;
using Microsoft.UI.Xaml;
using System.IO;
using Windows.ApplicationModel;

namespace AudioMerge.App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel MainViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel = new MainViewModel();
            Title = "Audio Merge";

            SetWindowAppIcon();
        }

        private void SetWindowAppIcon()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "AppIcon.ico"));
        }
    }
}
