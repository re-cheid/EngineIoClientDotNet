
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Modules;
using System;



namespace Quobject.EngineIoClientDotNet.Thread
{
    public class EasyTimer
    {


        private CancellationTokenSource ts;


        public EasyTimer(CancellationTokenSource ts)
        {
            this.ts = ts;
        }


        public static EasyTimer SetTimeout(Action method, int delayInMilliseconds)
        {
            return SetTimeout(method, delayInMilliseconds, false);
        }

        public static EasyTimer SetTimeout(Action method, int delayInMilliseconds, bool isPing)
        {
            var ts = new CancellationTokenSource();
            var ct = ts.Token;

            if (isPing)
            {
                new System.Threading.Thread(() =>
                {
                    var log = LogManager.GetLogger(Global.CallerName());
                    if (isPing)
                    {
                        log.Info("PING delay start: " + ts.IsCancellationRequested);
                    }
                    new ManualResetEvent(false).WaitOne(delayInMilliseconds);
                    //System.Threading.Thread.Sleep(delayInMilliseconds);
                    if (isPing)
                    {
                        log.Info("PING delay done: " + ts.IsCancellationRequested);
                    }
                    if (!ts.IsCancellationRequested)
                    {
                        method();
                        log.Info("PING worker completed, cancelled: " + ts.IsCancellationRequested);
                    }
                }).Start();
            }
            else
            {
                var worker = new BackgroundWorker();

                worker.DoWork += (s, e) => System.Threading.Thread.Sleep(delayInMilliseconds);
                worker.RunWorkerCompleted += (s, e) =>
                {
                    var log = LogManager.GetLogger(Global.CallerName());
                    if (isPing)
                    {
                        log.Info("PING worker completed, cancelled: " + ts.IsCancellationRequested);
                    }

                    if (!ts.IsCancellationRequested)
                    {

                        // Task.Factory.StartNew(method, ct, TaskCreationOptions.None, TaskScheduler.Default);
                        if (isPing)
                        {
                            log.Info("PING task created, cancelled: " + ts.IsCancellationRequested);
                        }
                    }
                };

                worker.RunWorkerAsync();
            }


                // Returns a stop handle which can be used for stopping
                // the timer, if required
                return new EasyTimer(ts);
        }

        public void Stop()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("EasyTimer stop");
            if (ts != null)
            {
                ts.Cancel();
            }          
        }

        public static void TaskRun(Action action)
        {
            Task.Run(action).Wait();
        }

        public static Task TaskRunNoWait(Action action)
        {
            return Task.Run(action);
        }

    }


}

