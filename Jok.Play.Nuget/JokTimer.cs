using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Jok.Play
{
    class JokTimerInternal<T> : IJokTimer<T>, IGlobalTimer
    {
        Timer timer;

        public void SetInterval(Action<T> method, T state, int delayInMilliseconds)
        {
            Stop();

            timer = new Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) =>
            {
                method(state);
            };

            timer.Enabled = true;
            timer.Start();
        }

        public void SetTimeout(Action<T> method, T state, int delayInMilliseconds)
        {
            Stop();

            timer = new System.Timers.Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) =>
            {
                method(state);
            };

            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();
        }

        public void Stop()
        {
            try
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Close();
                    timer.Dispose();
                    timer = null;
                }
            }
            catch { }
        }
    }



    public static class JokTimer<T>
    {
        public static IJokTimer<T> Create()
        {
            return new JokTimerInternal<T>();
        }
    }

    public interface IJokTimer<T>
    {
        void SetInterval(Action<T> method, T state, int delayInMilliseconds);
        void SetTimeout(Action<T> method, T state, int delayInMilliseconds);
        void Stop();
    }

    interface IGlobalTimer
    {
        void Stop();
    }

    public static class Global
    {
        public static object SetTimeout<T>(Action<T> method, TimeSpan interval, T state = default(T))
        {
            var timer = new JokTimerInternal<T>();
            timer.SetTimeout(method, state, Convert.ToInt32(interval.TotalMilliseconds));

            return (IGlobalTimer)timer;
        }

        public static object SetInterval<T>(Action<T> method, TimeSpan interval, T state = default(T))
        {
            var timer = new JokTimerInternal<T>();
            timer.SetInterval(method, state, Convert.ToInt32(interval.TotalMilliseconds));

            return (IGlobalTimer)timer;
        }

        public static void ClearTimeout(object timer)
        {
            var item = (timer as IGlobalTimer);
            if (item == null) return;

            item.Stop();
        }

        public static void ClearInterval(object timer)
        {
            ClearTimeout(timer);
        }
    }
}
