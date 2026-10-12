using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Windows;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SKSwapChainPanelOpenGLUnloadedIssue;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private bool _running;
    private static IntPtr _openGlHandle;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void StressButton_OnClick(object sender, RoutedEventArgs e)
    {
        await RunStressAsync(StressButton, "Start add/remove stress");
    }

    private async void PinnedStressButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Workaround: keep opengl32.dll loaded for the lifetime of the process.
        // The handle is intentionally never freed, so the DLL can no longer be
        // unloaded/reloaded between SKSwapChainPanel GL context creations.
        if (_openGlHandle == IntPtr.Zero)
        {
            _openGlHandle = NativeLibrary.Load("opengl32.dll");
        }

        await RunStressAsync(PinnedStressButton, "Start stress (opengl32 pinned)");
    }

    private async Task RunStressAsync(Button button, string idleContent)
    {
        if (_running)
        {
            _running = false;
            return;
        }

        _running = true;
        button.Content = "Stop";
        StressButton.IsEnabled = button == StressButton;
        PinnedStressButton.IsEnabled = button == PinnedStressButton;
        var iteration = 0;

        while (_running)
        {
            MainPanelHost.Children.Clear();
            await Task.Delay(Random.Shared.Next(0, 50));

            var panel = new SKSwapChainPanel();
            panel.PaintSurface += MainPanel_OnPaintSurface;
            MainPanelHost.Children.Add(panel);
            await Task.Delay(Random.Shared.Next(0, 50));

            StatusText.Text = $"Iteration {++iteration} | opengl32 pinned: {_openGlHandle != IntPtr.Zero}";
        }

        button.Content = idleContent;
        StressButton.IsEnabled = true;
        PinnedStressButton.IsEnabled = true;
    }

    private void MainPanel_OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
    {
        e.Surface.Canvas.Clear(SKColors.CornflowerBlue);
    }

}
