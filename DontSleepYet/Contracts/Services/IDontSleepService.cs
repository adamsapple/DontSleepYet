using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontSleepYet.Contracts.Services;

public interface IDontSleepService
{
    /// <summary>  
    /// Initializes the service.  
    /// </summary>  
    Task InitializeAsync();

    void Initialize();
    /// <summary>  
    /// Starts the service to prevent the system from sleeping.  
    /// </summary>  
    Task StartAsync();
    /// <summary>  
    /// Stops the service to allow the system to sleep again.  
    /// </summary>  
    Task StopAsync();
    /// <summary>  
    /// Checks if the service is currently active.  
    /// </summary>  
    bool IsActive { get; set; }

    /// <summary>  
    /// 眠らないようにする処理の感覚  
    /// </summary>  
    int WakeUpDurationSeconds { get; set; }
}
