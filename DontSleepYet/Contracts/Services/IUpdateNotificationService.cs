using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontSleepYet.Contracts.Services;
public interface IUpdateNotificationService
{
    Task StartAsync();
    Task StopAsync();
    void IgnoreThisVersion();
    Task CheckAndNotificationAsync();
    void TestShow();
}
