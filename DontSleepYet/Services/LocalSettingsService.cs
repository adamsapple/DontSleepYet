using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DontSleepYet.Contracts.Services;
using DontSleepYet.Core.Contracts.Services;
using DontSleepYet.Core.Helpers;
using DontSleepYet.Helpers;
using DontSleepYet.Models;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Windows.ApplicationModel;
using Windows.Storage;

namespace DontSleepYet.Services;

public class LocalSettingsService : ILocalSettingsService
{
    private const string _defaultApplicationDataFolder = "DontSleepYet/ApplicationData";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService _fileService;
    //private readonly LocalSettingsOptions _options;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;
    private readonly string _localsettingsFile;

    private IDictionary<string, object> _settings;

    private bool _isInitialized;
    private readonly ReaderWriterLockSlim fileLlock = new();        //< ファイルアクセス用ロック

    private readonly LocalSettingsOptions LocalSettingsOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void LocalSettingsOptions_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized)
        {
            return;
        }

        if (e.PropertyName is null)
        {
            return;
        }

        if (sender != LocalSettingsOptions)
        {
            return;
        }

        //var type = sender!.GetType()!;
        //var prop = type.GetProperty(e.PropertyName!);

        //var val = prop.GetValue(sender);

        //await SaveSettingAsync(e.PropertyName, val);

        //Debug.WriteLine($"LocalSettingsOptions_OnPropertyChanged: {e.PropertyName}");        
        //await SaveSettingAsync(e.PropertyName!, sender!.GetType().GetProperty(e.PropertyName!)!.GetValue(sender));
        await SaveSettingAsync(LocalSettingsOptions);
    }

    public LocalSettingsService(IFileService fileService, LocalSettingsOptions options, IOptions<LocalSettingsOptions> defaultOptions)
    {
        _fileService = fileService;
        
        options.IsDontSleepActive              = defaultOptions.Value.IsDontSleepActive;
        options.LocalSettingsFile              = defaultOptions.Value.LocalSettingsFile;
        options.ApplicationDataFolder          = defaultOptions.Value.ApplicationDataFolder; 
        options.DontSleepWakeUpDurationSeconds = defaultOptions.Value.DontSleepWakeUpDurationSeconds;

        options.PropertyChanged += LocalSettingsOptions_OnPropertyChanged;


        _applicationDataFolder = Path.Combine(_localApplicationData, options.ApplicationDataFolder ?? _defaultApplicationDataFolder);
        _localsettingsFile = options.LocalSettingsFile ?? _defaultLocalSettingsFile;

        _settings = new Dictionary<string, object>();

        LocalSettingsOptions = options;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return; 
        }
        
        _settings = await Task.Run(() => 
            {
                fileLlock.EnterReadLock();
                try
                {
                    var result = _fileService.Read<IDictionary<string, object>>(_applicationDataFolder, _localsettingsFile) ?? new Dictionary<string, object>();

                    return result;
                }
                finally
                {
                    fileLlock.ExitReadLock();
                }
            });

        //var type       = LocalSettingsOptions.GetType();
        //var properties = type.GetProperties();
        //foreach (var key in _settings.Keys.Where(x => x != null))
        //{

        //    if (!properties.Any(x => x.Name == key))
        //    {
        //        continue;
        //    }
        //    var prop = properties
        //                    .First(x => x.Name == key);
        //    if(prop != null)
        //    {
        //        prop.SetValue(key, _settings[key]);
        //        Debug.WriteLine($"Load setting: {key} = {_settings[key]}");
        //    }
        //}


        var readed = _fileService.Read<LocalSettingsOptions>(_applicationDataFolder, _localsettingsFile);
        var props = LocalSettingsOptions.GetType().GetProperties();

        if (readed is not null)
        {
            foreach (var prop in props)
            {
                prop.SetValue(LocalSettingsOptions, prop.GetValue(readed));
            }
        }

        _isInitialized = true;        
    }

    //public async Task<T?> ReadSettingAsync<T>(string key)
    //{
    //    //if (RuntimeHelper.IsMSIX)
    //    //{
    //    //    if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
    //    //    {
    //    //        return await Json.ToObjectAsync<T>((string)obj);
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    await InitializeAsync();

    //    //    if (_settings != null && _settings.TryGetValue(key, out var obj))
    //    //    {
    //    //        return await Json.ToObjectAsync<T>((string)obj);
    //    //    }
    //    //}
    //    //
    //    //return default;

    //    return await ReadSettingAsync<T?>(key, default);
    //}

    public async Task<T?> ReadSettingAsync<T>(string key, T? defaultValue = default)
    {        
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }
        else
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }

        return defaultValue;
    }

    public async Task SaveSettingAsync<T>(string key, T value)
    {
        if (RuntimeHelper.IsMSIX)
        {
            ApplicationData.Current.LocalSettings.Values[key] = Json.Stringify(value);
        }
        else
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            _settings[key] = await Json.StringifyAsync(value);

            await Task.Run(() => {
                fileLlock.EnterWriteLock();
                try
                {
                    _fileService.Save(_applicationDataFolder, _localsettingsFile, _settings);
                }
                finally
                {
                    fileLlock.ExitWriteLock();
                }
            });
        }
    }

    public async Task SaveSettingAsync<T>(T value)
    {
        Debug.WriteLine("SaveSettingAsync");

        await Task.Run(() =>
        {
            fileLlock.EnterWriteLock();
            try
            {
                _fileService.Save<T>(_applicationDataFolder, _localsettingsFile, value);
            }
            finally
            {
                fileLlock.ExitWriteLock();
            }
        });
    }
}
