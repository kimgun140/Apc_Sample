using NAudio.CoreAudioApi;
using NAudio.Wave;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Apc_Sample
{
    internal class ApcSampleClass
    {
        public BlockingCollection<Action> _workQueue = new();

        public List<ScheduleDetail> schedules = new List<ScheduleDetail>();

        private string TimeStamp = DateTime.Now.ToString("HHMMss000");
        //현재시간 비교용 
        public Stopwatch stopwatch = Stopwatch.StartNew();

        public ScheduleDetail NowProgram { get; set; }

        public ScheduleDetail NextProgram { get; set; }

        CachedSound NextcachedSound;

        CachedSound NowcachedSound;

        private EventHandler apcEventHandler;
        public event EventHandler APCEventHandler
        {
            add
            {
                apcEventHandler += value;
            }
            remove
            {
                apcEventHandler -= value;
            }
        }




        public delegate void APCEventHandler1();

        public event APCEventHandler1 TriggerEvent;


        public delegate void AsyncAPCEventHandler(object sender, EventArgs e);

        public event AsyncAPCEventHandler AsyncTriggerEvent;




        //public delegate Task AsyncTestEvent(object sender, EventArgs e);
        public delegate Task AsyncTestEvent(object sender, AudioEventargs e);


        public event AsyncTestEvent AsyncSomething; //비동기 이벤트 핸들러 

        public void ScheduleLoad()
        // 스케줄 로드  
        {
            string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                //string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250515'";
                string querry = $" SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250515' ORDER BY SDDT_NUM";
                while (true)
                {
                    Console.WriteLine("스케줄 로드 시작");

                    List<ScheduleDetail> scheduleDetails = new List<ScheduleDetail>();

                    using (var command = new OracleCommand(querry, conn))
                    {

                        using (var reader = command.ExecuteReader())
                        {

                            while (reader.Read())// 읽을게 있는동안 
                            {
                                ScheduleDetail scheduleDayDetail = new ScheduleDetail();
                                // 대입용 리스트 
                                //Console.WriteLine($" {reader["SDDT_TITLE"]}");

                                scheduleDayDetail.SDDT_RUNTIME = reader["SDDT_RUNTIME"].ToString();
                                scheduleDayDetail.SDDT_SCDYDATE = reader["SDDT_SCDYDATE"].ToString();
                                scheduleDayDetail.SDDT_BRDTIME = reader["SDDT_BRDTIME"].ToString();
                                scheduleDayDetail.SDDT_TITLE = reader["SDDT_TITLE"].ToString();
                                scheduleDayDetail.SDDT_NUM = Convert.ToInt32(reader["SDDT_NUM"]);

                                scheduleDetails.Add(scheduleDayDetail);
                            }
                        }
                        schedules = scheduleDetails;
                        //schedules = scheduleDetails;

                    }

                    Console.WriteLine("스케줄 가져오기 완료");
                    Thread.Sleep(5000);
                }
            }
        }


        public async Task CursorMethod()//스케줄 데이터 업데이트
        {

            //Stopwatch Teststopwatch = Stopwatch.StartNew();

            while (true)
            {
                int schedulesCount = 0;

                if (schedules.Count == 0)
                {
                    Console.WriteLine("스케줄이 없습니다. 로드 중");
                    Thread.Sleep(1000);
                }
                foreach (var item in schedules)
                {

                    var runtime = double.Parse(item.SDDT_RUNTIME); // 예: "11000" 밀리세컨드
                    var runtime1 = int.Parse(item.SDDT_RUNTIME); // 예: "11000" 밀리세컨드

                    var Runtimespan = TimeSpan.FromMilliseconds(runtime);
                    //double runtime = 5740000; // 밀리세컨드
                    int minutes = runtime1 / 100000;
                    int seconds = (runtime1 / 1000) % 100;
                    TimeSpan time = new TimeSpan(0, 0, minutes, seconds);

                    // 분 단위로 출력
                    double totalMinutes = Runtimespan.TotalMinutes;
                    var StratTime = StringToDateTime(item.SDDT_BRDTIME);
                    var EndTime = StratTime + time;

                    if (DateTime.Now >= item.StartTime && DateTime.Now < EndTime)
                    {
                        item.Now_Playing = true;
                        NowProgram = item;
                        NextProgram = schedules[schedulesCount + 1];//이거이거
                        //+1
                    }
                    else
                    {
                        item.Now_Playing = false;
                    }



                    schedulesCount++;
                }
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd/mm/ss/fff}");

                Console.WriteLine($"방송중인 프로그램 : {NowProgram?.SDDT_TITLE}");
                Console.WriteLine($"다음 프로그램 : {NextProgram?.SDDT_TITLE}");
                Thread.Sleep(1000); // 더 정밀한 주기로 변경해야하나 이것도 시간을 정확히 맞춰야할까 

            }

        }



        public void EventTimer()
        {

            Stopwatch WatchisWatch = new Stopwatch();
            bool isLoaded = false;
            TimeSpan Timediff;
            double SecDiff = 0.0;
            while (true)
            {
                //Task.Delay(100);
                //var NowNow = DateTime.Now;
                if ((SecDiff = (NextProgram.StartTime - DateTime.Now).TotalSeconds) <= 5 && isLoaded == false)
                {
                    isLoaded = true;
                    WatchisWatch.Start();
                    NextcachedSound = new CachedSound(@"20241201120000_녹음12.wav");
                    Console.WriteLine($"음악 파일 로딩 시간: {WatchisWatch.ElapsedMilliseconds}");
                    Console.WriteLine($"SecDiff: {SecDiff}");
                    Console.WriteLine($"Load: NextProgram: {NextProgram.StartTime} - {DateTime.Now} ");
                    WatchisWatch.Stop();
                    //var NowPro = new CachedSound(@"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201220000_녹음22.wav");
                }
                if ((Timediff = (NextProgram.StartTime - DateTime.Now)).TotalMilliseconds <= 50.0)// 시작시간 50ms안으로 들어오면 1ms단위로 반복 
                {
                    Task.Run(() =>
                    {
                        Console.WriteLine(Timediff);
                        Console.WriteLine(" Load1");
                        WatchisWatch.Restart();
                        long currentMs = 0;
                        while (true)
                        {

                            if (WatchisWatch.ElapsedMilliseconds > currentMs)
                            {
                                currentMs = WatchisWatch.ElapsedMilliseconds;
                                Console.WriteLine($"{currentMs}");

                                //if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 1.0)// 
                                if (currentMs >= 49)// 49
                                {
                                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - {NextProgram.SDDT_TITLE} 방송 시작 이벤트 발생");

                                    AudioEventargs audioEventargs = new AudioEventargs();

                                    audioEventargs.NowCachedProgram = NowcachedSound;
                                    audioEventargs.NextProgram = NextcachedSound;
                                    AsyncSomething?.Invoke(this, audioEventargs);//
                                    WatchisWatch.Stop();
                                    Console.WriteLine("이벤트끝");

                                    break;
                                }
                            }
                        }
                    });
                    break;
                }
            }
        }
        public async Task EventMethhod123()
        {
            Stopwatch WatchisWatch = Stopwatch.StartNew();
            bool isLoaded = false;


            while (true)
            {
                DateTime referenceTime = DateTime.Now;

                var currentTime = referenceTime + WatchisWatch.Elapsed;

                if ((NextProgram.StartTime - currentTime).TotalMilliseconds <= 10000.0 && !isLoaded)
                {
                    isLoaded = true;
                    NextcachedSound = new CachedSound(@"20241201120000_녹음12.wav");
                    //cachedsound가 어떤식으로 동작하는지 확인해야지 

                    Console.WriteLine("Load");
                }

                if ((NextProgram.StartTime - currentTime).TotalMilliseconds <= 50.0)
                {
                    Console.WriteLine("Load2");

                    WatchisWatch.Restart();
                    referenceTime = DateTime.Now; // 기준 시간 재설정
                    long currentMs = 0;

                    while (true)
                    {
                        Console.WriteLine("Load3");

                        var nowTime = referenceTime + WatchisWatch.Elapsed;

                        if (WatchisWatch.ElapsedMilliseconds > currentMs)
                        {
                            currentMs = WatchisWatch.ElapsedMilliseconds;
                            Console.WriteLine($"{currentMs}");

                            if ((NextProgram.StartTime - nowTime).TotalMilliseconds <= 3.0)
                            {
                                Console.WriteLine("Load4");

                                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");

                                AudioEventargs audioEventargs = new AudioEventargs
                                {
                                    NowCachedProgram = NowcachedSound,
                                    NextProgram = NextcachedSound
                                };

                                AsyncSomething?.Invoke(this, audioEventargs);
                                return;
                            }
                        }

                        await Task.Yield(); // CPU 점유 줄이기
                    }
                }

                await Task.Delay(5); // 불필요한 루프 회피
            }
        }




        public async Task WatchMethod1()// 원래 꺼 
        {
            Thread.Sleep(3000);
            while (true)
            {
                TimeSpan diff = NextProgram.StartTime - DateTime.Now;

                if (diff.TotalMilliseconds > 50)
                {
                    Thread.Sleep(50); // 넉넉하게 기다리기
                }
                else
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    long lastMs = 0;
                    while (true)
                    {
                        if (sw.ElapsedMilliseconds > lastMs)
                        {
                            lastMs = sw.ElapsedMilliseconds;

                            double delta = Math.Abs((NextProgram.StartTime - DateTime.Now).TotalMilliseconds);

                            if (delta <= 1.0)
                            {
                                // 정확한 타이밍 도달
                                //await AsyncSomething?.Invoke(this, EventArgs.Empty);
                                break;
                            }

                            if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds < -10)
                            {
                                // 놓쳤거나 지났으면 루프 탈출
                                break;
                            }
                        }
                    }
                }
            }


        }
        public string filePath = "";
        public WasapiOut? wasapiOut;
        public AudioFileReader? audioFileReader;




        static void BusyWait(double milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalMilliseconds < milliseconds)
            {
                Console.WriteLine($"{sw.ElapsedMilliseconds}");
            }
        }

        public int ConvertRuntimeToMilliseconds(string runtimeStr)
        {
            //string runtimeStr = "005808000";

            int hours = int.Parse(runtimeStr.Substring(0, 2));
            int minutes = int.Parse(runtimeStr.Substring(2, 2));
            int seconds = int.Parse(runtimeStr.Substring(4, 2));
            int milliseconds = int.Parse(runtimeStr.Substring(6, 3));

            int totalMilliseconds =
                (hours * 3600 * 1000) +
                (minutes * 60 * 1000) +
                (seconds * 1000) +
                milliseconds;

            return totalMilliseconds;




        }







        public DateTime StringToDateTime(string SDDT_BRDTIME)
        {
            DateTime ProgramstartTime = DateTime.Today
            .AddHours(int.Parse(SDDT_BRDTIME.Substring(0, 2)))
            .AddMinutes(int.Parse(SDDT_BRDTIME.Substring(2, 2)))
            .AddSeconds(int.Parse(SDDT_BRDTIME.Substring(4, 2)));
            return ProgramstartTime;

        }
        public void PlayMethod()
        // 이벤트
        {
            IWaveProvider waveProvider = null;


        }



        public class AudioEventargs : EventArgs
        {
            public CachedSound NowCachedProgram { get; set; }
            public CachedSound NextProgram { get; set; }
            public CachedSound BeforeProgram { get; set; }


            public ISampleProvider SampleProvider { get; set; }
        }


    }
}
