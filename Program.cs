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


            var cachedSound1 = new CachedSound(@"20241201120000_녹음12.wav");
            CachedSoundSampleProvider sampleProvider1 = new CachedSoundSampleProvider(cachedSound1);
            myMixingSampleProvider myMixingSampleProvider = new myMixingSampleProvider(sampleProvider1.WaveFormat);
            //myMixingSampleProvider myMixingSampleProvider = new myMixingSampleProvider(44100, 2);
            // 송출될 파일의 포맷을 알아야겠다. bit depth는 변경해줌 

            myMixingSampleProvider.ReadFully = true;
            myMixingSampleProvider.AddMixerInput(sampleProvider1);
            var outputdevice = new WaveOutEvent();



            // 재생할 오디오 데이터가 없어도 재생하게 

            //myMixingSampleProvider.AddMixerInput(sampleProvider1);
            outputdevice.Init(myMixingSampleProvider);
            outputdevice.Play();


            Thread LoadThread = new Thread(() =>
            {
                apcSampleClass.ScheduleLoad();
            });
            LoadThread.Start();

            // 생산자 스레드 이벤트발생 시킴 
            Thread CursorThread = new Thread(() =>
            {
                //apcSampleClass.NewCursorCheck_Print();
                apcSampleClass.CursorMethod();
            });
            CursorThread.Start();

            Thread WatchThread = new Thread(() =>
            {
                Thread.Sleep(5000);
                apcSampleClass.EventTimer();
                //apcSampleClass.EventMethhod123();
            }
            );
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
                //bool eventflag = false;
                //var eventflag1 = 0;
                if ((now - lastInvokeTime) < throttleInterval)
                {
                    lastInvokeTime = now;

                    //eventflag = true;
                    // 간격 안쪽에 발생한 이벤트는 무시 
                    return;
                }
                Console.WriteLine($"now: {now}, lastInvokeTime: {lastInvokeTime}, diff: {now - lastInvokeTime}, ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                eventcount1++;
                lastInvokeTime = now;

                try
                {
                    Console.WriteLine("event try");
                    CachedSound cachedSound = e.NextProgram;
                    CachedSoundSampleProvider cachedSoundSampleProvider = new CachedSoundSampleProvider(cachedSound);

                    myMixingSampleProvider.RemoveAllMixerInputs();
                    myMixingSampleProvider.AddMixerInput(cachedSoundSampleProvider);
                    Console.WriteLine($"오디오 Done");

                    Task.Run(() =>
                    {
                        Thread.Sleep(5000);
                        apcSampleClass.EventTimer();
                    });
                    Console.WriteLine("Task.run after");
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
