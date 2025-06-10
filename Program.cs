using NAudio.Wave.SampleProviders;
using NAudio.Wave;
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
     
            AudioPlaybackEngine AudioPlayer = new AudioPlaybackEngine(44100, 2);
            
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
                apcSampleClass.EventMethhod();

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

                    CachedSound cachedSound = e.NextProgram;
                    // 이벤트에 현재꺼 다음거는 최소한 전달을해야한다. 
                    // 오디오를 제거하고, 다음거 추가
                    // or 전부다 삭제하고 다음꺼를 추가하기 

                    //AudioPlayer.RemoveMixerInput(new CachedSoundSampleProvider(NowProgram));
                    // Isampleprovider를 전달한다는게 오디오 파일 ㅊ자체를 전달한다는게 아니다 .
                    AudioPlayer.PlaySound(cachedSound);
                    // 이벤트 발생 횟수 통제 해야함 
                    //AudioPlaybackEngine.Instance.PlaySound();
                    Task.Run(() => { apcSampleClass.EventMethhod(); });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Task.Run(() => { apcSampleClass.EventMethhod(); });
                }

            };

        }

    }
}
