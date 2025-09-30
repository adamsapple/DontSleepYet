namespace DontSleepYet.Contracts.Services;

public interface ILocalSettingsService
{
    Task InitializeAsync();
    Task<T?> ReadSettingAsync<T>(string key, T? defaultValue = default);
    //Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
}
