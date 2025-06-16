using DontSleepYet.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace DontSleepYet.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.InitializeCommand.Execute(null);
    }
}
