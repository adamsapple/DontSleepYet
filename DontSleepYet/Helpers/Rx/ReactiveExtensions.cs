using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DontSleepYet.Helpers.Rx;
public static class ReactiveExtensions
{
    public static IObservable<T> ThrottleLatest<T>(this IObservable<T> source, TimeSpan interval)
    {
        return ThrottleLatest(source, interval, TimeSpan.FromSeconds(2));
    }

    public static IObservable<T> ThrottleLatest<T>(this IObservable<T> source, TimeSpan interval, TimeSpan timeOut)
    {
        return Observable.Create<T>(observer =>
        {
            object gate = new();
            var latestValue   = default(T)!;
            var hasValue      = false;
            var isThrottling  = false;
            var lastEmittedAt = DateTimeOffset.MinValue;

            IDisposable? timeSubscription = null;
            IDisposable? idleSubscription = null;

            Action emitLatest = () =>
            {
                if (hasValue)
                {
                    //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} => {latestValue} 排出");

                    observer.OnNext(latestValue);
                    hasValue      = false;
                    lastEmittedAt = DateTimeOffset.UtcNow;
                }
            };

            Action disposeTimer = () =>
            {
                //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} DisposeTimer.");

                timeSubscription?.Dispose();
                idleSubscription?.Dispose();

                timeSubscription = null;
                idleSubscription = null;

                isThrottling = false;
            };

            Action startIdleTimer = () => 
            {
                //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} IdleTimer.開始");

                idleSubscription?.Dispose();
                var idleInterval = timeOut + interval;
                idleSubscription = Observable.Timer(idleInterval, idleInterval).Subscribe(_ =>
                {
                    //var endTime = lastEmittedAt + interval + timeOut;
                    //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} Idle {endTime.ToString("HH:mm:ss.fff")}");
                    lock (gate)
                    {
                        if (lastEmittedAt + idleInterval >= DateTimeOffset.UtcNow)
                        {
                            return;
                        }

                        disposeTimer();
                    }
                });
            };

            var subscription = source.Subscribe(value =>
            {
                lock (gate)
                {
                    //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} => {value}");

                    latestValue = value;
                    hasValue    = true;

                    if (isThrottling)
                    {
                        return;
                    }

                    emitLatest();
                    
                    isThrottling = true;

                    /// 1つ目の値が来たらTimerを起動
                    timeSubscription = Observable.Timer(interval, interval).Subscribe(_ =>
                    {
                        /// 未送信の値があれば最新を送信
                        lock (gate)
                        {
                            //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} => interval({hasValue})");
                            emitLatest();
                            
                            //isThrottling = false;
                        }
                    });

                    // idleTimerは必要な時だけ起動
                    if (idleSubscription is null)
                    {
                        startIdleTimer();
                    }
                }
            },
            ex =>
            {
                lock (gate)
                {
                    disposeTimer();
                    observer.OnError(ex);
                }
            },
            () =>
            {
                lock (gate)
                {
                    emitLatest();
                    disposeTimer();
                    observer.OnCompleted();
                }
            });

            return subscription;
        });
    }

}
