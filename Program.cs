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
                apcSampleClass.EventMethod();
                //apcSampleClass.EventMethhod123();
            }
            );
            Thread.Sleep(5000);
            WatchThread.Start();
            //WatchThread.
            //apcSampleClass.AsyncSomething += async (sender, AudioEventargs) =>
            var eventcount = 0;
            var eventcount1 = 0;

            DateTime lastInvokeTime = DateTime.MinValue;
            TimeSpan throttleInterval = TimeSpan.FromMilliseconds(1000);// 


            apcSampleClass.AsyncSomething += async (sender, e) =>
            {
                eventcount++;
                var now = DateTime.Now;
                bool eventflag = false;
                var eventflag1 = 0;
                if ((now - lastInvokeTime) > throttleInterval)
                {
                    lastInvokeTime = now;

                    eventflag = true;
                    // 간격 안쪽에 발생한 이벤트는 무시 
                    return;
                }
                Console.WriteLine($"now: {now}, lastInvokeTime: {lastInvokeTime}, diff: {now - lastInvokeTime}, ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                eventcount1++;
                lastInvokeTime = now;

                try
                {
                    CachedSound cachedSound = e.NextProgram;
                    CachedSoundSampleProvider cachedSoundSampleProvider = new CachedSoundSampleProvider(cachedSound);
                    //AudioPlayer.RemoveMixerInput(new CachedSoundSampleProvider(NowProgram));
                    var plz = AudioPlayer.ConvertToRightChannelCount(cachedSoundSampleProvider);

                    AudioPlayer.RemoveMixerInput(plz);

                    AudioPlayer.PlaySound(cachedSound);

                    AudioPlayer.RemoveMixerInput(plz);

                    //AudioPlayer.mixer.RemoveMixerInput(cachedSoundSampleProvider);
                    //AudioPlayer.RemoveMixerInput(cachedSoundSampleProvider);

                    await Task.Delay(5000);
                    Task.Run(() =>
                    {
                        apcSampleClass.EventMethod();
                    });
                    //Task.Run(() => { apcSampleClass.EventMethhod123(); });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    //Task.Run(() => { apcSampleClass.EventMethod(); });
                    //Task.Run(() => { apcSampleClass.EventMethhod123(); });
                }

            };

        }

    }
}
