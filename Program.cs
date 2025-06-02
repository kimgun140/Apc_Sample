using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Apc_Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ApcSampleClass apcSampleClass = new ApcSampleClass();
            //DateTime _lastInvokeTime = DateTime.MinValue;
            // 이거이거
            //TimeSpan _throttleInterval = TimeSpan.FromSeconds(1);



            //apcSampleClass.AsyncTriggerEvent += (sender, EventArgs) =>
            //{
            //    //첫 시도에는 어떻게 되네 
            //    var now = DateTime.Now;
            //    if ((now - _lastInvokeTime) < _throttleInterval)
            //    {
            //        // 스로틀 시간 간격 내면 무시
            //        return;
            //    }
            //    _lastInvokeTime = now;


            //    apcSampleClass._workQueue.Add(async () =>
            //    {
            //        try
            //        {
            //            apcSampleClass.ScheduleLoadWithArg(sender, EventArgs.Empty);
            //            //await Task.Delay(1000);
            //            //Thread.Sleep(100);
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine(e);
            //        }

            //        ////apcEventHandler(this, EventArgs.Empty);// 생성 
            //    });

            //};

            //CancellationTokenSource cts = new CancellationTokenSource();
            //DateTime _lastScheduled = DateTime.MinValue;
            ////DateTime timerStarted = DateTime.UtcNow.AddYears(-1);
            //Stopwatch stopwatch = new Stopwatch();





            //object _throttleLock = new object();
            DateTime _lastInvokeTime = DateTime.MinValue;
            TimeSpan _throttleInterval = TimeSpan.FromSeconds(1);

            apcSampleClass.AsyncTriggerEvent += (sender, EventArgs) =>
                {
                    var now = DateTime.Now;

                    if ((now - _lastInvokeTime) < _throttleInterval)
                    {
                        // 스로틀 시간 간격 내면 무시
                        return;
                    }

                    _lastInvokeTime = now;

                    //이벤트가 발생하지만, 실제 핸들러까지 호출되지는 않는다. 
                    Task.Run(async () =>
                    {
                        try
                        // 
                        {
                            apcSampleClass.ScheduleLoadWithArg(sender, EventArgs.Empty);
                            await Task.Delay(1000); 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    });
                };





            //apcSampleClass.AsyncTriggerEvent += (sender, EventArgs) =>
            //{
            //    apcSampleClass._workQueue.Add(async () =>
            //    {
            //        // 이벤트가 발생할 때 정확한 시간에 발생하게 하기 

            //        try
            //        {
            //            apcSampleClass.ScheduleLoadWithArg(sender, EventArgs.Empty);
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine(e);
            //        }
            //    });

            //};

            //// BlockingCollection 워크 큐에 담긴 작업들을 차례대로 실행한다.
            //// 소비자 스레드 이벤트 처리
            //Task.Run(() =>
            //{
            //    foreach (var work in apcSampleClass._workQueue.GetConsumingEnumerable())
            //    {
            //        try
            //        {
            //            work();
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"예외 발생: {ex.Message}");
            //        }
            //        //Thread.Sleep(10000);
            //    }
            //});

            Thread WatchThread = new Thread(apcSampleClass.WatchStart);
            WatchThread.Start();


            Thread LoadThread = new Thread(() =>
            {
                apcSampleClass.ScheduleLoad();
            });
            LoadThread.Start();

            // 생산자 스레드 이벤트발생 시킴 
            Thread CursorThread = new Thread(() =>
            {
                apcSampleClass.ScheduleCheck_Print();
            });
            CursorThread.Start();



            //apcSampleClass.APCEventHandler += apcSampleClass.ScheduleChanging;

            //apcSampleClass.APCEventHandler += apcSampleClass.TirggeredEvent;



            ////  Task로 병럴 핸들러 처리 
            //apcSampleClass.AsyncTriggerEvent += (sender, e) =>
            //{
            //    {
            //        Task.Run(() =>
            //        apcSampleClass.EventHandling(sender, EventArgs.Empty));
            //    }
            //};
            ////



            // 이벤트 발생 => 스케줄 로드  
            //apcSampleClass.APCEventHandler += apcSampleClass.ScheduleLoad;

            //apcSampleClass.TriggerEvent += () =>
            //{
            //    apcSampleClass._workQueue.Add(() =>
            //    {
            //        apcSampleClass.ScheduleLoad();

            //        //apcEventHandler(this, EventArgs.Empty);// 생성 
            //    });
            //};

            // 이스레드는 BlockingCollection 워크 큐에 담긴 작업들을 차례대로 실행한다.
            // 소비자 스레드 이벤트 처리 
            //Task.Run(() =>
            //{
            //    foreach (var work in apcSampleClass._workQueue.GetConsumingEnumerable())
            //    {
            //        try { work(); }
            //        catch (Exception ex) { Console.WriteLine($"예외 발생: {ex.Message}"); }
            //    }
            //});


            // 이벤트 비동기 호출  
            //apcSampleClass.AsynSomething += async (sender, e) =>
            //{
            //    {
            //        try
            //        {
            //            await apcSampleClass.EventHandling(sender, e);
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"{ex}");
            //        }
            //    }
            //};

            ////
            // Task.Run()으로 비동기 처리
            //apcSampleClass.AsyncTriggerEvent += (sender, e) =>
            //{
            //    {
            //        Task.Run(() =>
            //        {
            //            apcSampleClass.EventHandling1(sender, e);
            //        });
            //    }
            //};
            ////

            //apcSampleClass.APCEventHandler += apcSampleClass.EventOuccur;

            // 이벤트 발생 => 스케줄 로드  
            //apcSampleClass.APCEventHandler += apcSampleClass.ScheduleLoad;







        }

    }
}
