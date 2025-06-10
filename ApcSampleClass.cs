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

        // 방송런타임 측정용 
        private int NowProRuntime;
        private int NowProBrdTime;

        CachedSound NextPro;



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
        // 스케줄 로드 스레드 1초마다 
        {
            string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250515'";
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

                                scheduleDetails.Add(scheduleDayDetail);
                            }
                        }
                        schedules = scheduleDetails;
                    }


                    Console.WriteLine("스케줄 가져오기 완료");
                    Thread.Sleep(5000);
                }
            }
        }
        public Task ScheduleLoadWithArg(object sender, EventArgs e)
        // 이벤트용 스케줄 로드 
        {
            Console.WriteLine("방송 시작시간 이벤트 시작");

            string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250527'";


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

                            scheduleDayDetail.runtimeMilliSec = ConvertRuntimeToMilliseconds(scheduleDayDetail.SDDT_RUNTIME);

                            scheduleDetails.Add(scheduleDayDetail);
                        }
                    }

                    schedules = scheduleDetails;


                    return Task.CompletedTask;
                    //Console.WriteLine("스케줄 가져오기 완료");

                }
            }
        }

        public async Task EventHandling(object sender, EventArgs e)
        // 변성표 
        {

            Console.WriteLine("이벤트 시작 스케줄 가져오기 시작");

            string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250515'";

                Console.WriteLine("스케줄 로드 시작");

                List<ScheduleDetail> scheduleDetails = new List<ScheduleDetail>();

                using (var command = new OracleCommand(querry, conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())// 읽을게 있는동안 
                        {
                            //Console.WriteLine("이벤트 처리중");
                            ScheduleDetail scheduleDayDetail = new ScheduleDetail();

                            scheduleDayDetail.SDDT_RUNTIME = reader["SDDT_RUNTIME"].ToString();
                            scheduleDayDetail.SDDT_SCDYDATE = reader["SDDT_SCDYDATE"].ToString();
                            scheduleDayDetail.SDDT_BRDTIME = reader["SDDT_BRDTIME"].ToString();
                            scheduleDayDetail.SDDT_TITLE = reader["SDDT_TITLE"].ToString();

                            scheduleDetails.Add(scheduleDayDetail);
                        }
                    }
                    schedules = scheduleDetails;
                }


                /*      await */
                Task.Delay(15000);
                //Thread.Sleep(10000);
                // 여기서 락이 걸리는건 비동기 작업중인 커서 스레드 자체를 멈추기 때문
                Console.WriteLine("이벤트 완료 스케줄 가져오기 완료");
                Console.WriteLine("이벤트 완료 스케줄 가져오기 완료");


            }
        }
        public void EventHandling1(object sender, EventArgs e)
        // 변성표 
        {

            Console.WriteLine("이벤트 시작 스케줄 가져오기 시작");

            string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250515'";

                Console.WriteLine("스케줄 로드 시작");

                List<ScheduleDetail> scheduleDetails = new List<ScheduleDetail>();

                using (var command = new OracleCommand(querry, conn))
                {

                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())// 읽을게 있는동안 
                        {
                            //Console.WriteLine("이벤트 처리중");
                            ScheduleDetail scheduleDayDetail = new ScheduleDetail();

                            scheduleDayDetail.SDDT_RUNTIME = reader["SDDT_RUNTIME"].ToString();
                            scheduleDayDetail.SDDT_SCDYDATE = reader["SDDT_SCDYDATE"].ToString();
                            scheduleDayDetail.SDDT_BRDTIME = reader["SDDT_BRDTIME"].ToString();
                            scheduleDayDetail.SDDT_TITLE = reader["SDDT_TITLE"].ToString();

                            scheduleDetails.Add(scheduleDayDetail);
                        }
                    }
                    schedules = scheduleDetails;
                }


                /*      await */
                Task.Delay(15000);
                //Thread.Sleep(10000);
                // 여기서 락이 걸리는건 비동기 작업중인 커서 스레드 자체를 멈추기 때문
                Console.WriteLine("이벤트 완료 스케줄 가져오기 완료");
                Console.WriteLine("이벤트 완료 스케줄 가져오기 완료");


            }
        }
        public async Task ScheduleCheck_Print()// 현재 시간 커서 표시  
        {
            //커서 표시하기  NowPlaying 
            while (true)
            {
                if (schedules.Count == 0)
                {
                    Console.WriteLine("스케줄이 없습니다. 로드 중");
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    foreach (var item in schedules)
                    // 리스트를 복사해서 사용하는거랑 
                    {
                        int brdTime = int.Parse(item.SDDT_BRDTIME);
                        int runTime = int.Parse(item.SDDT_RUNTIME) / 1000;
                        int nowTime = int.Parse(DateTime.Now.ToString("HHmmss"));

                        if (nowTime >= brdTime && brdTime + runTime > nowTime)
                        //if ((brdTime) >= (int.Parse(DateTime.Now.ToString("HHmm00")) + int.Parse(stopwatch.Elapsed.Milliseconds.ToString())))
                        {
                            item.Now_Playing = true; // 방송중인 프로그램 플래그
                            NowProgram = item;
                            //if (brdTime == nowTime)

                            if ((brdTime) * 1000 <= (int.Parse(DateTime.Now.ToString("HHmmss000")) + int.Parse(stopwatch.Elapsed.Milliseconds.ToString())))
                            // 시작시간이 됐을 때 이벤트 발생 
                            // 이게 위로가면 현재 방송중인 프로그램 보다 뒤 시간에 있는건 전부다 발생한다. 
                            //범위를 시간 차를 계산 해서 줄일까 
                            {
                                Console.WriteLine($"{TimeStamp}, {stopwatch.Elapsed.Seconds}");
                                //if (AsynSomething != null)
                                if (AsyncTriggerEvent != null)
                                {
                                    AsyncTriggerEvent.Invoke(this, EventArgs.Empty);
                                    //이벤트 
                                }
                            }
                        }
                        else
                        {
                            item.Now_Playing = false; // 방송중이지 않은 프로그램 플래그
                        }

                    }
                    Console.WriteLine($"방송중인 프로그램 : {NowProgram.SDDT_TITLE}");
                    //TirggeredEvent();
                }
                Thread.Sleep(10); // 10ms 대기

            }

        }


        DateTime _lastInvokeTime = DateTime.MinValue;
        TimeSpan _throttleInterval = TimeSpan.FromMilliseconds(10);
        //Stopwatch stopwatch = Stopwatch.StartNew(); // ScheduleCheck_Print 시작 시점 기준

        public async Task TESTScheduleCheck_Print()
        {
            Stopwatch Teststopwatch = Stopwatch.StartNew();
            while (true)
            {
                Teststopwatch.Restart();
                if (schedules.Count == 0)
                {
                    Console.WriteLine("스케줄이 없습니다. 로드 중");
                    Thread.Sleep(1000);
                }

                var now = DateTime.Now;


                foreach (var item in schedules)
                {
                    // 방송시작 시간으로 변환 
                    DateTime startTime = DateTime.Today
                        .AddHours(int.Parse(item.SDDT_BRDTIME.Substring(0, 2)))
                        .AddMinutes(int.Parse(item.SDDT_BRDTIME.Substring(2, 2)))
                        .AddSeconds(int.Parse(item.SDDT_BRDTIME.Substring(4, 2)));

                    int runtimeSec = int.Parse(item.SDDT_RUNTIME) / 1000;
                    DateTime endTime = startTime.AddSeconds(runtimeSec);

                    if (now >= startTime && now < endTime)// 이 조건을 어떻게 하라고했었지 
                    {
                        item.Now_Playing = true;
                        NowProgram = item;
                        // 시간을 비교해서 확인해야지

                        // 시간 계산  
                        var elapsedSinceStart = (now - startTime).TotalMilliseconds;
                        //

                        if (elapsedSinceStart >= 0 && elapsedSinceStart <= 15.0)// 15 ms로 측정 1.0ms는 필터링이 안된다. datetime이랑 비교해서  15ms차이가 나니까
                        {
                            Console.WriteLine($"{elapsedSinceStart:HH:mm:ss.fff} - 시간 차이");

                            if ((DateTime.Now - _lastInvokeTime) >= _throttleInterval)
                            {
                                _lastInvokeTime = DateTime.Now;

                                Console.WriteLine($"{now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");

                                AsyncTriggerEvent?.Invoke(this, EventArgs.Empty);
                            }
                        }
                    }
                    else
                    {
                        item.Now_Playing = false;
                    }
                    //Teststopwatch;
                }
                Console.WriteLine($"{Teststopwatch.ElapsedMilliseconds}");// 이게 한바퀴에 0밀리세컨드안이

                Console.WriteLine($"방송중인 프로그램 : {NowProgram?.SDDT_TITLE}");
                Thread.Sleep(1); // 더 정밀한 주기로 변경
            }
        }

        //public void NewCursorCheck_Print()
        //{

        //    Stopwatch Teststopwatch = Stopwatch.StartNew();
        //    //BroadCatsStarted
        //    while (true)
        //    {

        //        if (schedules.Count == 0)
        //        {
        //            Console.WriteLine("스케줄이 없습니다. 로드 중");
        //            Thread.Sleep(1000);
        //        }
        //        foreach (var item in schedules)
        //        {
        //            var now = DateTime.Now;
        //            //시간으로 변환  brdtime
        //            DateTime startTime = DateTime.Today
        //            .AddHours(int.Parse(item.SDDT_BRDTIME.Substring(0, 2)))
        //            .AddMinutes(int.Parse(item.SDDT_BRDTIME.Substring(2, 2)))
        //            .AddSeconds(int.Parse(item.SDDT_BRDTIME.Substring(4, 2)));

        //            // 방송 런타임 (ms)
        //            var runtimeMilliSec = ConvertRuntimeToMilliseconds(item.SDDT_RUNTIME);// 스트링 밀리세컨드로 변환 
        //            TimeSpan runtime = TimeSpan.FromMilliseconds(runtimeMilliSec);
        //            DateTime endTime = startTime.Add(runtime);

        //            // 방송 시작 시점 이후 경과 시간
        //            TimeSpan alreadyElapsed = now - startTime;
        //            //현재시간이랑 방송시작시간의 차이 방송시작한지 7초 지남 

        //            var actualElapsed = alreadyElapsed + Teststopwatch.Elapsed;
        //            // (현재시간 - 방송 시작시간) + 스탑워치 진행시간


        //            if (now >= startTime && now < endTime)// 이 runtimesec이 맞고 
        //            {
        //                item.Now_Playing = true;
        //                NowProgram = item;
        //            }

        //            if (Math.Abs(actualElapsed.TotalMilliseconds - runtimeMilliSec) <= 1.0)//  재생시간(밀리세컨드) 
        //            {
        //                Console.WriteLine($"{Teststopwatch.ElapsedMilliseconds:HH:mm:ss.fff} - 방송 시작 이벤트 발생");
        //                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");


        //                Console.WriteLine($"actualElapsed.TotalMilliseconds: {actualElapsed.TotalMilliseconds}, {DateTime.Now:HH:mm:ss.fff}");
        //                Console.WriteLine($"[검증] actualElapsed: {actualElapsed.TotalMilliseconds}ms / 런타임: {runtimeMilliSec}ms");

        //                AsyncTriggerEvent?.Invoke(this, EventArgs.Empty);

        //            }
        //        }
        //        Console.WriteLine($"{DateTime.Now}");
        //        Console.WriteLine($"{NowProgram?.SDDT_SCDYDATE}");

        //        Console.WriteLine($"방송중인 프로그램 : {NowProgram?.SDDT_TITLE}");
        //        //Thread.Sleep(1); // 더 정밀한 주기로 변경

        //    }

        //}

        public async Task NewCursorCheck_Print()//스케줄 데이터 업데이트
        {

            Stopwatch Teststopwatch = Stopwatch.StartNew();

            while (true)
            {

                if (schedules.Count == 0)
                {
                    Console.WriteLine("스케줄이 없습니다. 로드 중");
                    Thread.Sleep(1000);
                }
                foreach (var item in schedules)
                {
                    var now = DateTime.Now;
                    //시간으로 변환  brdtime
                    DateTime startTime = DateTime.Today
                    .AddHours(int.Parse(item.SDDT_BRDTIME.Substring(0, 2)))
                    .AddMinutes(int.Parse(item.SDDT_BRDTIME.Substring(2, 2)))
                    .AddSeconds(int.Parse(item.SDDT_BRDTIME.Substring(4, 2)));


                    var runtimeMilliSec0 = ConvertRuntimeToMilliseconds(item.SDDT_RUNTIME);// 스트링 => 밀리세컨드  
                    TimeSpan runtime0 = TimeSpan.FromMilliseconds(runtimeMilliSec0);// 밀리세컨드 => 시간 
                    DateTime endTime0 = startTime.Add(runtime0);// 현재시간 + 재생시간 =끝나는 시간 = 다음 시작시간 
                    TimeSpan alreadyElapsed0 = now - startTime;// 현재시간 - 시작시간 = 간격  +면 지난거 -면 방송전

                    if (now >= startTime && now < endTime0)// 방송중 현재 방송중인 프로그램  
                    {
                        item.Now_Playing = true;
                        NowProgram = item;
                        NowProRuntime = int.Parse(NowProgram.SDDT_RUNTIME) / 1000;
                        NowProBrdTime = int.Parse(NowProgram.SDDT_BRDTIME);
                        //NowProgram.actualElapsed = alreadyElapsed0 + Teststopwatch.Elapsed; // 지난 시간 + 스톱워치 시간 이거이거 여기서 업데이트를 해야할 필요없잖아 

                        // 방송 런타임 (ms)
                        NowProgram.runtimeMilliSec = ConvertRuntimeToMilliseconds(item.SDDT_RUNTIME);// 스트링 밀리세컨드로 변환
                        TimeSpan runtime = TimeSpan.FromMilliseconds(NowProgram.runtimeMilliSec);
                        DateTime endTime = startTime.Add(runtime);

                        // 방송 시작 시점 이후 경과 시간
                        TimeSpan alreadyElapsed = now - startTime;
                        //현재시간이랑 방송시작시간의 차이 방송시작한지 7초 지남 
                    }

                    if (NowProBrdTime + NowProRuntime == int.Parse(item.SDDT_BRDTIME))// Next Program 
                    {
                        NextProgram = item;
                    }

                }
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd/fff}");

                Console.WriteLine($"방송중인 프로그램 : {NowProgram?.SDDT_TITLE}");
                Thread.Sleep(10000); // 주기 변경

            }

        }

        public async Task NewCursorCheck_Print11()//스케줄 데이터 업데이트
        {

            //Stopwatch Teststopwatch = Stopwatch.StartNew();

            while (true)
            {

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
                    //var EndTime = StratTime + Runtimespan;
                    var EndTime = StratTime + time;

                    if (DateTime.Now >= item.StartTime && DateTime.Now < EndTime)
                    {
                        item.Now_Playing = true;
                        NowProgram = item;
                        NowProRuntime = int.Parse(item.SDDT_RUNTIME);
                        NowProBrdTime = int.Parse(item.SDDT_BRDTIME);

                    }

                    else
                    {
                        item.Now_Playing = false;
                    }
                    //if (NowProRuntime / 1000 + NowProBrdTime == int.Parse(item.SDDT_BRDTIME))
                    if (NowProgram != null)
                    {
                        int convertedRuntime = ConvertRuntimeToMilliseconds(NowProgram.SDDT_RUNTIME);
                        DateTime expectedNext = StringToDateTime(NowProgram.SDDT_BRDTIME).AddMilliseconds(convertedRuntime);
                        if (expectedNext == item.StartTime)
                        {
                            NextProgram = item;
                            item.Now_Playing = false;
                        }
                    }
                }
                Console.WriteLine($"{DateTime.Now:yyyy/MM/dd/ss/fff}");

                Console.WriteLine($"방송중인 프로그램 : {NowProgram?.SDDT_TITLE}");
                Thread.Sleep(1000); // 더 정밀한 주기로 변경

            }

        }


        public async Task WatchMethod()//원래꺼
        {

            Stopwatch WatchisWatch = new Stopwatch();

            Thread.Sleep(3000);// 첫시작 업데이트 기다리기

            while (true)
            {
                // 비교는 now , next 시간을 비교해서 1ms 이하일때 
                var runtimeMilliSec0 = ConvertRuntimeToMilliseconds(NowProgram.SDDT_RUNTIME);// 스트링 밀리세컨드로 변환
                TimeSpan runtime0 = TimeSpan.FromMilliseconds(runtimeMilliSec0);
                DateTime endTime0 = NowProgram.StartTime.Add(runtime0);
                TimeSpan alreadyElapsed0 = DateTime.Now - NowProgram.StartTime;
                NowProgram.actualElapsed = alreadyElapsed0 + WatchisWatch.Elapsed;

                //Thread.Sleep(20);//20ms 
                if (Math.Abs(NowProgram.actualElapsed.TotalMilliseconds - NowProgram.runtimeMilliSec) <= 0.05)// 시작시간 50ms안으로 들어오면 1ms단위로 반복 
                {// 전체 재생시간이랑 현재 재생지점의 차이가 
                    Thread.Sleep(50);

                    WatchisWatch.Restart();
                    long currentMs = 0;
                    while (true)
                    {

                        runtimeMilliSec0 = ConvertRuntimeToMilliseconds(NowProgram.SDDT_RUNTIME);// 스트링 밀리세컨드로 변환
                        runtime0 = TimeSpan.FromMilliseconds(runtimeMilliSec0);
                        endTime0 = NowProgram.StartTime.Add(runtime0);
                        alreadyElapsed0 = DateTime.Now - NowProgram.StartTime;
                        NowProgram.actualElapsed = alreadyElapsed0 + WatchisWatch.Elapsed;

                        if (WatchisWatch.ElapsedMilliseconds > currentMs)
                        {
                            currentMs = WatchisWatch.ElapsedMilliseconds;
                            Console.WriteLine($"{currentMs}");
                            // 스탑워치시간이랑 
                            //if ((Math.Abs(NowProgram.actualElapsed.TotalMilliseconds - WatchisWatch.ElapsedMilliseconds) <= 1.0))
                            if ((Math.Abs(NowProgram.actualElapsed.TotalMilliseconds - NowProgram.runtimeMilliSec) <= 1.0))// NowProgram.actualElapsed이거 업데이트가 늦는데 
                            {
                                //if ((Math.Abs(NowProgram.actualElapsed.TotalMilliseconds - NowProgram.runtimeMilliSec) <= 1.0))
                                //    {
                                Console.WriteLine($"{WatchisWatch.ElapsedMilliseconds:HH:mm:ss.fff} - 방송 시작 이벤트 발생");
                                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");

                                //AsyncSomething?.Invoke(this, EventArgs.Empty);//이벤트 하나 더 
                                //AsyncSomething?.Invoke(this, EventArgs.Empty);//이벤트 하나 더 

                                break;

                            }
                        }
                    }
                }
                // 보장시간 10-15ms


                //Thread.Sleep(50);

            }
        }
        //public int eventcount;
        public async Task WatchMethod11()
        {

            Stopwatch WatchisWatch = new Stopwatch();

            //Thread.Sleep(5000);// 첫시작 업데이트 기다리기
            //WatchisWatch.Restart();
            while (true)
            {
                //Thread.Sleep(10);
                if (/*50.0 < (NextProgram.StartTime - DateTime.Now).TotalMilliseconds &&*/ (NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 200.0)
                {
                    //WatchisWatch.Restart();
                    NextPro = new CachedSound(@"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201120000_녹음12.wav");
                    Console.WriteLine(" ");
                    //var NowPro = new CachedSound(@"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201220000_녹음22.wav");
                }
                //Console.WriteLine("---------");
                //Thread.Sleep(10);
                //if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 50.0)// 시작시간 50ms안으로 들어오면 1ms단위로 반복 
                //{
                //    //  
                //    // 미리 로드 


                //    WatchisWatch.Restart();
                //    long currentMs = 0;
                //    while (true)
                //    {

                //        if (WatchisWatch.ElapsedMilliseconds > currentMs)
                //        {
                //            currentMs = WatchisWatch.ElapsedMilliseconds;
                //            Console.WriteLine($"{currentMs}");

                //            if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 1.0)// 
                //            {
                //                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");

                //                //AsyncSomething?.Invoke(this, EventArgs.Empty);//
                //                AudioEventargs audioEventargs = new AudioEventargs();
                //                audioEventargs.CachedSound = NextPro;
                //                AsyncSomething?.Invoke(this, audioEventargs);//

                //                break;

                //            }
                //        }
                //        //Console.WriteLine($"1ms 루프 시간: {WatchisWatch.ElapsedMilliseconds}");//

                //    }
                //    break;

                //}
                if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 50.0)
                {
                    WatchisWatch.Restart();
                    long currentMs = 0;
                    while (true)
                    {
                        if (WatchisWatch.ElapsedMilliseconds > currentMs)
                        {
                            currentMs = WatchisWatch.ElapsedMilliseconds;

                            if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 1.0)
                            {
                                // 방송 시작 이벤트 발생
                                AudioEventargs audioEventargs = new AudioEventargs();
                                audioEventargs.CachedSound = NextPro;
                                AsyncSomething?.Invoke(this, audioEventargs);
                                break;
                            }
                        }
                    }
                    break;
                }
                //Console.WriteLine($" watch {WatchisWatch.ElapsedMilliseconds}");//
            }
        }


        public async Task WatchMethod_Cached()
        {

            Stopwatch WatchisWatch = new Stopwatch();

            //Thread.Sleep(5000);// 첫시작 업데이트 기다리기

            while (true)
            {
                Thread.Sleep(1000);
                if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 180.0)
                {
                    NextPro = new CachedSound(@"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201120000_녹음12.wav");

                    //var NowPro = new CachedSound(@"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201220000_녹음22.wav");
                    // 여기서 로드하면 지연되겠지  
                }



                if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 50.0)// 시작시간 50ms안으로 들어오면 1ms단위로 반복 
                {
                    //  
                    // 미리 로드 


                    WatchisWatch.Restart();
                    long currentMs = 0;
                    while (true)
                    {

                        if (WatchisWatch.ElapsedMilliseconds > currentMs)
                        {
                            currentMs = WatchisWatch.ElapsedMilliseconds;
                            Console.WriteLine($"{currentMs}");

                            if ((NextProgram.StartTime - DateTime.Now).TotalMilliseconds <= 1.0)// 
                            {
                                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");
                                AudioEventargs audioEventargs = new AudioEventargs();
                                audioEventargs.CachedSound = NextPro;
                                AsyncSomething?.Invoke(this, audioEventargs);//

                                //AsyncSomething?.Invoke(this, EventArgs.Empty);//

                                break;

                            }
                        }
                    }
                    break;

                }
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

        public void WasapiPlay()
        {
            filePath = @"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201120000_녹음12.wav";
            string NextProgramFilePath = @"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201220000_녹음22.wav";
            try
            {
                if (wasapiOut != null && wasapiOut.PlaybackState == PlaybackState.Playing)// 
                {
                    wasapiOut.Stop();
                }

                audioFileReader = new AudioFileReader(filePath); // 로드 
                wasapiOut = new WasapiOut(AudioClientShareMode.Shared, false, 100);// 출력 설정 
                // AudioClientShareMode.Shared: 사운다 카드 공유모드 오디올르 자동으로 리샘프링한다. 
                // eventsync: 오디오 플레이하는 백그라운드 스레드 동작제어 , true 추가 오디오 원할 때 이벤트 수신, false 잠시 대기후 오디오 제공 
                // Latency 지연시간 
                wasapiOut.Init(audioFileReader);
                Console.WriteLine($"{NowProgram.SDDT_TITLE}, {NowProgram.FilePath}");


                wasapiOut?.Play();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //MessageBox.Show(ex.Message);
            }
        }
        public void AudioPlaySample()
        {
            filePath = @"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201120000_녹음12.wav";
            string NextProgramFilePath = @"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201220000_녹음22.wav";


            try
            {
                using (var audioFile = new AudioFileReader(filePath))
                using (var outputDevice = new WasapiOut())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(1000);
                    }
                }

            }
            catch (Exception ex)
            {


            }
        }


        static void BusyWait(double milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalMilliseconds < milliseconds)
            {
                // Spin
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



        //public event AsyncAPCEventHandler2 AsyncTriggerEvent3;





        //public void ChangeSchedule(EventArgs eventArgs)
        //{
        //    //AsyncTriggerEvent2.Invoke(this, eventArgs);

        //    string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

        //    using (var conn = new OracleConnection(ConnectionString))
        //    {

        //        conn.Open();
        //        while (true)
        //        {

        //            string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250527'";



        //            Console.WriteLine("스케줄 변경");

        //            List<ScheduleDetail> scheduleDetails = new List<ScheduleDetail>();

        //            using (var command = new OracleCommand(querry, conn))
        //            {

        //                using (var reader = command.ExecuteReader())
        //                {

        //                    while (reader.Read())// 읽을게 있는동안 
        //                    {
        //                        ScheduleDetail scheduleDayDetail = new ScheduleDetail();
        //                        // 대입용 리스트 
        //                        //Console.WriteLine($" {reader["SDDT_TITLE"]}");

        //                        scheduleDayDetail.SDDT_RUNTIME = reader["SDDT_RUNTIME"].ToString();
        //                        scheduleDayDetail.SDDT_SCDYDATE = reader["SDDT_SCDYDATE"].ToString();
        //                        scheduleDayDetail.SDDT_BRDTIME = reader["SDDT_BRDTIME"].ToString();
        //                        scheduleDayDetail.SDDT_TITLE = reader["SDDT_TITLE"].ToString();

        //                        scheduleDetails.Add(scheduleDayDetail);
        //                        //schedules.Add(scheduleDayDetail);
        //                        // 대입용 리스트 
        //                    }
        //                }

        //                //lock (lockObj)
        //                //{
        //                NewSchedules = scheduleDetails;
        //                schedules = NewSchedules;
        //                //여기여기 
        //                //}
        //            }

        //            Thread.Sleep(10000);
        //            Console.WriteLine("스케줄 변경 완료");

        //        }
        //    }
        //}


        public void PrintNowProgram()
        {

            foreach (var item in schedules)
            {
                //Console.WriteLine($"방송 날짜 : {item.SDDT_SCDYDATE}");
                //Console.WriteLine($"방송 시작 시간 : {item.SDDT_BRDTIME}");
                //Console.WriteLine($"방송 종료 시간 : {item.SDDT_RUNTIME}");
                //Console.WriteLine($"방송 제목 : {item.SDDT_TITLE}");
                if (item.Now_Playing == true)
                {
                    Thread.Sleep(100); // 1초 대기
                    Console.WriteLine($"방송중인 프로그램 : {item.SDDT_TITLE}");
                }
                //else
                //{
                //    Console.WriteLine($"방송중이지 않은 프로그램 : {item.SDDT_TITLE}");
                //}

            }


        }


        public class APCEventHandlerEventArgs : EventArgs
        {
            public int EventId { get; set; }
            public DateTime TimeReached { get; set; }
        }

        public void EventOuccur(object sender, EventArgs e)// 
        {
            //ScheduleLoad(sender, e);
            //if (this.apcEventHandler != null)
            //{
            //    // 이벤트핸들러들을 호출
            //    apcEventHandler(this, EventArgs.Empty);
            //}

            Console.WriteLine("event 발생");
            Thread.Sleep(10000);

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
            var zap = new CachedSound("zap.wav");
            var boom = new CachedSound("boom.wav");


        }
        AudioPlaybackEngine audioPlaybackEngine = new AudioPlaybackEngine();


        public class AudioEventargs : EventArgs
        {
            public CachedSound CachedSound { get; set; }

        }


    }
}
