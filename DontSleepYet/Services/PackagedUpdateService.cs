using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DontSleepYet.Contracts.Services;

namespace DontSleepYet.Services;
public class PackagedUpdateService : IUpdateService
{
    public Task UpdateAsync(Uri uri) => throw new NotImplementedException();
}
