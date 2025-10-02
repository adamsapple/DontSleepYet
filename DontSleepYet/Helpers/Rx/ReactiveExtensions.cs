using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontSleepYet.Helpers.Rx;
public static class ReactiveExtensions
{
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
                    observer.OnNext(latestValue);
                    hasValue      = false;
                    lastEmittedAt = DateTimeOffset.UtcNow;
                }
            };

            Action disposeTimer = () =>
            {
                timeSubscription?.Dispose();
                idleSubscription?.Dispose();
            };

            Action startIdleTimer = () => 
            { 
                idleSubscription?.Dispose();
                idleSubscription = Observable.Timer(interval+TimeSpan.FromSeconds(1)).Subscribe(_ =>
                {
                    lock (gate)
                    {
                        if (lastEmittedAt + interval + timeOut >= DateTimeOffset.UtcNow)
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
                    latestValue = value;
                    hasValue    = true;

                    if (isThrottling)
                    {
                        return;
                    }

                    emitLatest();
                    
                    isThrottling = true;

                    /// 1つ目の値が来たらTimerを起動
                    timeSubscription = Observable.Timer(interval).Subscribe(_ =>
                    {
                        /// 未送信の値があれば最新を送信
                        lock (gate)
                        {
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
