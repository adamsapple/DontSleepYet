using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Helpers;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace DontSleepYet.ViewModels;
public partial class AboutViewModel : ObservableRecipient
{
    #region Properties.
    [ObservableProperty]
    private string _versionDescription;
    #endregion Properties.

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    public AboutViewModel()
    {
        _versionDescription = GetVersionDescription();
    }
}
