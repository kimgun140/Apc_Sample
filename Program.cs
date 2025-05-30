using System.ComponentModel;
using System.Threading.Tasks;

namespace Apc_Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ApcSampleClass apcSampleClass = new ApcSampleClass();



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

            apcSampleClass.AsyncTriggerEvent += (sender, EventArgs) =>
            {


                apcSampleClass._workQueue.Add(async () =>
                {
                    try
                    {
                        apcSampleClass.ScheduleLoadWithArg(sender, EventArgs.Empty);
                        await Task.Delay(1000);
                        // 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    ////apcEventHandler(this, EventArgs.Empty);// 생성 
                });

            };

            // BlockingCollection 워크 큐에 담긴 작업들을 차례대로 실행한다.
            // 소비자 스레드 이벤트 처리
            Task.Run(() =>
            {
                foreach (var work in apcSampleClass._workQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        work();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"예외 발생: {ex.Message}");
                    }
                    //Thread.Sleep(10000);
                }
            });




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







        }

    }
}
