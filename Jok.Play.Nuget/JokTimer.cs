using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Jok.GameEngine
{
    class JokTimerInternal<T> : IJokTimer<T>
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
}
