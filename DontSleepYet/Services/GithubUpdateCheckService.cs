using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DontSleepYet.Contracts.Services;

namespace DontSleepYet.Services;

public class GithubUpdateCheckService : IUpdateCheckService
{
    public Task<UpdateCheckData> CheckUpdateAsync() => throw new NotImplementedException();
}
