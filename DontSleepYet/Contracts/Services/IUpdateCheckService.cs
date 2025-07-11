﻿using System;
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
    public DateTimeOffset PublishedAt { get; }
    public string? Description { get; }
    public Uri? InfoUrl { get; }

    public UpdateCheckData(bool isCheckSuccess, bool isUpdateAvailable, string? latestVersion, DateTimeOffset publishedAt, string? description, Uri? infoUrl)
    {
        IsCheckSuccess    = isCheckSuccess;
        IsUpdateAvailable = isUpdateAvailable;
        LatestVersion     = latestVersion;
        PublishedAt       = publishedAt;
        Description       = description;
        InfoUrl           = infoUrl;
    }
}
