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


            DateTime _lastInvokeTime = DateTime.MinValue;
            TimeSpan _throttleInterval = TimeSpan.FromMilliseconds(20);// 0.01

            apcSampleClass.AsyncTriggerEvent += (sender, EventArgs) =>
                {
                    var now = DateTime.Now;

                    if ((now - _lastInvokeTime) < _throttleInterval)
                    {
                        // 간격 안쪽에 발생한 이벤트는 무시 / 현재시간 - 마지막 이벤트 발생시간 =< 간격보다 작을 때 무시  
                        return;
                    }

                    _lastInvokeTime = now;

                    //이벤트가 발생
                    Task.Run(async () =>
                    {
                        try
                        // 
                        {
                            apcSampleClass.ScheduleLoadWithArg(sender, EventArgs.Empty);
                            //await Task.Delay(1000); 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    });
                };

            apcSampleClass.AsyncSomething += async (sender, EventArgs) =>
            {
                var now = DateTime.Now;

                if ((now - _lastInvokeTime) < _throttleInterval)
                {
                    // 간격 안쪽에 발생한 이벤트는 무시 / 현재시간 - 마지막 이벤트 발생시간 =< 간격보다 작을 때 무시  
                    return;
                }

                _lastInvokeTime = now;

                //이벤트가 발생

                try
                // 
                {
                    await apcSampleClass.ScheduleLoadWithArg(sender, EventArgs.Empty);
                    //await Task.Delay(1000); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

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




            Thread LoadThread = new Thread(() =>
            {
                apcSampleClass.ScheduleLoad();
            });
            LoadThread.Start();

            // 생산자 스레드 이벤트발생 시킴 
            Thread CursorThread = new Thread(() =>
            {
                apcSampleClass.NewCursorCheck_Print();
            });
            CursorThread.Start();


            Thread WatchThread = new Thread(() =>
            {
                apcSampleClass.WatchMethod();
            }
            );
            WatchThread.Start();


        }

    }
}
