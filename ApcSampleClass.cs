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
        public Stopwatch stopwatch = new Stopwatch();

        public ScheduleDetail NowProgram { get; set; }

        public ScheduleDetail scheduleDetail { get; set; }

        public List<ScheduleDetail> NewSchedules { get; set; }

        //private readonly object lockObj = new object();
        //public ApcSampleClass()
        //{


        //    //stopwatch.Start();
        //}

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

        //public event EventHandler AsyncTriggerEvent2;

        //public delegate Task AsyncTestEvent(object sender, AsyncCompletedEventArgs e);

        public delegate Task AsyncTestEvent(object sender, EventArgs e);


        public event AsyncTestEvent AsyncSomething;

        public void ScheduleLoad()
        // 처음이랑 이벤트가 발생했을 때만 이걸 실행하면 됨 
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
        public void ScheduleLoadWithArg(object sender, EventArgs e)
        // 이벤트용 스케줄 로드 
        {
            string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250527'";
                //while (true)
                //{
                Console.WriteLine("이벤트");

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
                    //}

                    //Thread.Sleep(10000);
                    // 대기 

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
        public void WatchStart()
        {
            while (true)
            {
                if (DateTime.Now.Second == 00)
                {
                    stopwatch.Start();
                    return;
                }
            }
        }




        //public event AsyncAPCEventHandler2 AsyncTriggerEvent3;





        public void ChangeSchedule(EventArgs eventArgs)
        {
            //AsyncTriggerEvent2.Invoke(this, eventArgs);

            string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.245)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=oras)));User Id=WINNERS;Password=WINNERS009;";

            using (var conn = new OracleConnection(ConnectionString))
            {

                conn.Open();
                while (true)
                {

                    string querry = $"SELECT * FROM SCHEDAYDETAIL WHERE SDDT_SCDYDATE = '20250527'";



                    Console.WriteLine("스케줄 변경");

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
                                //schedules.Add(scheduleDayDetail);
                                // 대입용 리스트 
                            }
                        }

                        //lock (lockObj)
                        //{
                        NewSchedules = scheduleDetails;
                        schedules = NewSchedules;
                        //여기여기 
                        //}
                    }

                    Thread.Sleep(10000);
                    Console.WriteLine("스케줄 변경 완료");

                }
            }
        }


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
