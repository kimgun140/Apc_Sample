using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static Apc_Sample.ApcSampleClass;

namespace Apc_Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int eventcount = 0;

            ApcSampleClass apcSampleClass = new ApcSampleClass();


            DateTime lastInvokeTime = DateTime.MinValue;
            TimeSpan throttleInterval = TimeSpan.FromMilliseconds(1000);// 



            Thread LoadThread = new Thread(() =>
            {
                apcSampleClass.ScheduleLoad();
            });
            LoadThread.Start();

            // 생산자 스레드 이벤트발생 시킴 
            Thread CursorThread = new Thread(() =>
            {
                //apcSampleClass.NewCursorCheck_Print();
                apcSampleClass.NewCursorCheck_Print11();
            });
            CursorThread.Start();

            Thread WatchThread = new Thread(() =>
            {
                //apcSampleClass.WatchMethod();
                apcSampleClass.WatchMethod11();

            }
            );
            Thread.Sleep(5000);
            WatchThread.Start();
            //apcSampleClass.AsyncSomething += async (sender, AudioEventargs) =>

            apcSampleClass.AsyncSomething += async (sender, e) =>
            {
                var now = DateTime.Now;

                if ((now - lastInvokeTime) < throttleInterval)
                {
                    // 간격 안쪽에 발생한 이벤트는 무시 /
                    return;
                }

                lastInvokeTime = now;

                //이벤트 발생

                try
                // 
                {
                    //await apcSampleClass.ScheduleLoadWithArg(sender, EventArgs.Empty); // 이벤트 
                    //eventcount++;
                    //apcSampleClass.WasapiPlay();
                    CachedSound cachedSound = e.CachedSound;
                    AudioPlaybackEngine.Instance.PlaySound(cachedSound);

                    //AudioPlaybackEngine.Instance.PlaySound();
                    // 이벤트를 전달해서 처리, 아니면 자체에서 처리 
                    // 다른 스레드에서 작업 시작
                    Task.Run(() => { apcSampleClass.WatchMethod11(); });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Task.Run(() => { apcSampleClass.WatchMethod11(); });
                }

            };

        }

    }
}
