using NAudio.Wave;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private Stopwatch BroadCatsStarted = new Stopwatch();
        // 방송런타임 측정용 
        private int NowRuntime;
        private int NowBrdTime;




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




        public delegate Task AsyncTestEvent(object sender, EventArgs e);

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

                    Thread.Sleep(1000);
                    Console.WriteLine("스케줄 가져오기 완료");

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

                            scheduleDetails.Add(scheduleDayDetail);
                        }
                    }

                    schedules = scheduleDetails;
                    Console.WriteLine("이벤트 완료");

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

        public async Task NewCursorCheck_Print()
        {

            Stopwatch Teststopwatch = Stopwatch.StartNew();
            //stopwatch에서 datetime으로 처리? thread.sleep으로? 
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


                    var runtimeMilliSec0 = ConvertRuntimeToMilliseconds(item.SDDT_RUNTIME);// 스트링 밀리세컨드로 변환 
                    TimeSpan runtime0 = TimeSpan.FromMilliseconds(runtimeMilliSec0);
                    DateTime endTime0 = startTime.Add(runtime0);
                    TimeSpan alreadyElapsed0 = now - startTime;

                    if (now >= startTime && now < endTime0)// 이 runtimesec이 맞고 
                    {
                        item.Now_Playing = true;
                        NowProgram = item;
                        NowProgram.actualElapsed = alreadyElapsed0 + Teststopwatch.Elapsed;
                        // 값이 순서를 어케 고치냐 


                        // 이걸 쪼개서 쓰지않고 한번에 쓸수 있을텐데 이게 NowProgram이 필요해서 첫바퀴돌때는 다른값이 필요해 

                        // 방송 런타임 (ms)
                        NowProgram.runtimeMilliSec = ConvertRuntimeToMilliseconds(item.SDDT_RUNTIME);// 스트링 밀리세컨드로 변환
                        TimeSpan runtime = TimeSpan.FromMilliseconds(NowProgram.runtimeMilliSec);
                        DateTime endTime = startTime.Add(runtime);

                        // 방송 시작 시점 이후 경과 시간
                        TimeSpan alreadyElapsed = now - startTime;
                        //현재시간이랑 방송시작시간의 차이 방송시작한지 7초 지남 
                    }
                    // (현재시간 - 방송 시작시간) + 스탑워치 진행시간

                    //if (now >= startTime && now < endTime)// 이 runtimesec이 맞고 
                    //{
                    //    item.Now_Playing = true;
                    //    NowProgram = item;
                    //    NowProgram.actualElapsed = alreadyElapsed + Teststopwatch.Elapsed;
                    //    // 값이 순서를 어케 고치냐 

                    //}
                    if (NowProgram != null)
                    {
                        NowRuntime = int.Parse(NowProgram.SDDT_RUNTIME) / 1000;
                        NowBrdTime = int.Parse(NowProgram.SDDT_BRDTIME);

                    }
                    if (NowBrdTime + NowRuntime == int.Parse(item.SDDT_BRDTIME))// 이거이거 
                    {
                        NextProgram = item;
                    }


                    //여기가 분리 
                    //if (Math.Abs(actualElapsed.TotalMilliseconds - runtimeMilliSec) <= 1.0)//  재생시간(밀리세컨드) 
                    //    // 이걸 다른걸로 분리를 하는게 스레드를 만들어서 분리를 해야겠지 
                    //{
                    //    Console.WriteLine($"{Teststopwatch.ElapsedMilliseconds:HH:mm:ss.fff} - 방송 시작 이벤트 발생");
                    //    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");


                    //    Console.WriteLine($"actualElapsed.TotalMilliseconds: {actualElapsed.TotalMilliseconds}, {DateTime.Now:HH:mm:ss.fff}");
                    //    Console.WriteLine($"[검증] actualElapsed: {actualElapsed.TotalMilliseconds}ms / 런타임: {runtimeMilliSec}ms");

                    //    await AsyncSomething?.Invoke(this, EventArgs.Empty);

                    //}
                }
                Console.WriteLine($"{DateTime.Now}");
                Console.WriteLine($"{NowProgram?.SDDT_SCDYDATE}");

                Console.WriteLine($"방송중인 프로그램 : {NowProgram?.SDDT_TITLE}");
                //Thread.Sleep(1); // 더 정밀한 주기로 변경

            }

        }

        public async void WatchMethod()
        {

            Stopwatch WatchisWatch = new Stopwatch();

            Thread.Sleep(3000);// 첫시작은 우선 처음에 걸려야지 

            while (true)
            {
                Thread.Sleep(20);//20ms 
                if (Math.Abs(NowProgram.actualElapsed.TotalMilliseconds - NowProgram.runtimeMilliSec) <= 50)// 시작시간 50ms안으로 들어오면 1ms단위로 반복 
                {
                    WatchisWatch.Restart();

                    //while (true)// 1ms동안은 계속 반속
                    {
                        //while ((Math.Abs(NowProgram.actualElapsed.TotalMilliseconds - NowProgram.runtimeMilliSec) == 1.0)) // 
                        if ((Math.Abs(NowProgram.actualElapsed.TotalMilliseconds - NowProgram.runtimeMilliSec) == 1.0))
                        {
                            Console.WriteLine($"{WatchisWatch.ElapsedMilliseconds:HH:mm:ss.fff} - 방송 시작 이벤트 발생");
                            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - 방송 시작 이벤트 발생");


                            Console.WriteLine($"actualElapsed.TotalMilliseconds: {NowProgram.actualElapsed.TotalMilliseconds}, {DateTime.Now:HH:mm:ss.fff}");
                            Console.WriteLine($"[검증] actualElapsed: {NowProgram.actualElapsed.TotalMilliseconds}ms / 런타임: {NowProgram.runtimeMilliSec}ms");

                            await AsyncSomething?.Invoke(this, EventArgs.Empty);


                            //Thread.Sleep(1);
                        }
                    }
                    // 보장시간 10-15ms

                }
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
        //public void TirggeredEvent(object sender, EventArgs e)
        //{
        //    Console.WriteLine("press 'a' key to increase total");
        //    while (Console.ReadKey(true).KeyChar == 'a')
        //    {
        //        //Console.WriteLine("Press "A" ");
        //        if (this.apcEventHandler != null)
        //        {
        //            // 이벤트핸들러들을 호출
        //            apcEventHandler(this, EventArgs.Empty);
        //        }
        //        //Thread.Sleep(15000);

        //    }

        //    //}
        //}


        public void ChangeFormat(string SDDT_BRDTIME)
        {
            DateTime ProgramstartTime = DateTime.Today
            .AddHours(int.Parse(SDDT_BRDTIME.Substring(0, 2)))
            .AddMinutes(int.Parse(SDDT_BRDTIME.Substring(2, 2)))
            .AddSeconds(int.Parse(SDDT_BRDTIME.Substring(4, 2)));


        }
        public void PlayMethod()
        // 시점이 변경될 때 
        {
            string audioFileName = "";
            using (var audioFile = new AudioFileReader(audioFileName))
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
    }
}
