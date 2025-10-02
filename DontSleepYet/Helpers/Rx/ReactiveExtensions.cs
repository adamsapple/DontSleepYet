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
        return Observable.Create<T>(observer =>
        {
            object gate = new();
            var latestValue   = default(T)!;
            var hasValue      = false;
            var isThrottling  = false;
            var lastEmittedAt = DateTimeOffset.MinValue;

            IDisposable? timeSubscription = null;

            Action emitLatest = () =>
            {
                //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} => {latestValue} 排出");

                observer.OnNext(latestValue);
                hasValue      = false;
                lastEmittedAt = DateTimeOffset.UtcNow;
            };

            Action disposeTimer = () =>
            {
                //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} DisposeTimer.");

                timeSubscription?.Dispose();
                timeSubscription = null;

                isThrottling = false;
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
                            
                            if (hasValue)
                            {
                                emitLatest();
                            }
                            else
                            {
                                disposeTimer();
                            }
                        }
                    });
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
                    disposeTimer();
                    observer.OnCompleted();
                }
            });

            return subscription;
        });
    }

}
