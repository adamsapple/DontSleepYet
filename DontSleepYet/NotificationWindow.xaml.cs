using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using ABI.Microsoft.UI.Xaml;
using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DontSleepYet;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NotificationWindow : WindowEx
{
    public NotificationWindow()
    {
        InitializeComponent();

        //VisibilityChanged += NotificationWindow_VisibilityChanged;
        //PositionChanged += NotificationWindow_PositionChanged;
    }

    private void NotificationWindow_PositionChanged(object? sender, Windows.Graphics.PointInt32 e) 
    { 
        Debug.WriteLine($"NotificationWindow Position Changed: X={e.X}, Y={e.Y}");
    }

    private void NotificationWindow_VisibilityChanged(object sender, Microsoft.UI.Xaml.WindowVisibilityChangedEventArgs args)
    {
        if (args.Visible)
        {
            var window = this.AppWindow;
            var compositor = new Compositor();  // this.Compositor;
            var visual = compositor.CreateSpriteVisual();

            // DPIから物理ピクセルへ変換する行列を取得してそれぞれの長さを変換
            //var dpi = Microsoft.UI.Xaml.Window.Current.PixelsPerViewPixel;
            //this.AppWindow.Size.Height
            this.AppWindow.Move(new Windows.Graphics.PointInt32
            {
                //X = (int)(App.MainWindow.AppWindow.Position.X + App.MainWindow.AppWindow.Size.Width - this.AppWindow.Size.Width),
                //Y = (int)(App.MainWindow.AppWindow.Position.Y + App.MainWindow.AppWindow.Size.Height - this.AppWindow.Size.Height)
                X = 0,
                Y = 0
            });

            foreach (WindowsDisplayAPI.Display disp in WindowsDisplayAPI.Display.GetDisplays())
            {
                // プライマリモニターか
                if (disp.CurrentSetting.Position.X == 0 && disp.CurrentSetting.Position.Y == 0)
                {
                    //this.AppWindow.Move(new Windows.Graphics.PointInt32
                    //{
                    //    //X = (int)(App.MainWindow.AppWindow.Position.X + App.MainWindow.AppWindow.Size.Width - this.AppWindow.Size.Width),
                    //    //Y = (int)(App.MainWindow.AppWindow.Position.Y + App.MainWindow.AppWindow.Size.Height - this.AppWindow.Size.Height)
                    //    X = disp.CurrentSetting.Resolution.Width  - window.Size.Width,
                    //    Y = disp.CurrentSetting.Resolution.Height - window.Size.Height
                    //});

                    var from = new Vector2(x: window.Size.Width,
                                           y: window.Size.Height);
                    var to   = new Vector2(x: disp.CurrentSetting.Resolution.Width - window.Size.Width,
                                           y: disp.CurrentSetting.Resolution.Height - window.Size.Height);

                    //var animation = compositor.CreateCubicBezierEasingFunction(from, to);
                    var animation = compositor.CreateVector2KeyFrameAnimation();
                    animation.InsertKeyFrame(0.0f, from);
                    animation.InsertKeyFrame(1.0f, to);
                    animation.Duration = TimeSpan.FromMilliseconds(500);
                    animation.Target = "Offset";




                    visual.StartAnimation("Offset", animation);
                    return;
                }
            }

        }
    }

    private async void StartWindowMoveAnimation(Vector2 from, Vector2 to, int durationMs)
    {
        int steps = 60;
        int interval = durationMs / steps;

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            var easedT = EaseOutCubic(t);
            var current = Vector2.Lerp(from, to, easedT);
            this.AppWindow.Move(new Windows.Graphics.PointInt32((int)current.X, (int)current.Y));
            await Task.Delay(interval);
        }
        
    }
    
    private float EaseOutCubic(float t)
    {
        return 1 - MathF.Pow(1 - t, 3);
    }
}



