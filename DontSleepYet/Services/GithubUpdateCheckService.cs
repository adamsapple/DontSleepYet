using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DontSleepYet.Contracts.Services;

namespace DontSleepYet.Services;

public class GithubUpdateCheckService : IUpdateCheckService
{
    public DateTime LastCheckedAt 
    {
        get => throw new NotImplementedException();
        private set => throw new NotImplementedException();
    }

    public DateTime NextCheckAt
    {
        get => throw new NotImplementedException();
        private set => throw new NotImplementedException();
    }
    public TimeSpan CheckPeriod
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public Task<UpdateCheckData> CheckUpdateAsync() => throw new NotImplementedException();
}
