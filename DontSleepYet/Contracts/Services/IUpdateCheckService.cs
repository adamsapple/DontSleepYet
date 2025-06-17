using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontSleepYet.Contracts.Services;

public interface IUpdateCheckService
{
    Task<UpdateCheckData> CheckUpdateAsync();
    DateTime LastCheckedAt { get; }
    DateTime NextCheckAt { get; }
    TimeSpan CheckPeriod { get; set; }
}

public class UpdateCheckData
{
    public bool IsCheckSuccess { get; }
    public bool IsUpdateAvailable { get; }
    public string? LatestVersion { get; }
    public string? Description { get; }
    public Uri? InfoUrl { get; }

    public UpdateCheckData(bool isCheckSuccess, bool isUpdateAvailable, string? latestVersion, string? description, Uri? infoUrl)
    {
        IsCheckSuccess      = isCheckSuccess;
        IsUpdateAvailable   = isUpdateAvailable;
        LatestVersion       = latestVersion;
        Description         = description;
        InfoUrl             = infoUrl;
    }
}
