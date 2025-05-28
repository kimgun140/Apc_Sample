using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apc_Sample
{
    internal class ApcSampleClass
    {

        public List<ScheduleDetail> schedules = new List<ScheduleDetail>();

        public ScheduleDetail NowProgram { get; set; }

        public ScheduleDetail scheduleDetail { get; set; }

        private readonly object lockObj = new object();

        public void ScheduleLoad()
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

                                //Console.WriteLine($" {reader["SDDT_TITLE"]}");

                                scheduleDayDetail.SDDT_RUNTIME = reader["SDDT_RUNTIME"].ToString();
                                scheduleDayDetail.SDDT_SCDYDATE = reader["SDDT_SCDYDATE"].ToString();
                                scheduleDayDetail.SDDT_BRDTIME = reader["SDDT_BRDTIME"].ToString();
                                scheduleDayDetail.SDDT_TITLE = reader["SDDT_TITLE"].ToString();

                                scheduleDetails.Add(scheduleDayDetail);
                            }
                        }

                        schedules = scheduleDetails;
                        //여기여기 
                    }

                    Thread.Sleep(1000);
                    Console.WriteLine("스케줄 가져오기 완료");

                }
            }
        }
        public void ScheduleCheck_Print()// 현재 시간 커서 표시 
        {
            //여기서 반환하기  현재시간 프로그램을 
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

                    foreach (var item in schedules.ToList())
                    {
                        int brdTime = int.Parse(item.SDDT_BRDTIME);
                        int runTime = int.Parse(item.SDDT_RUNTIME) / 1000;
                        int nowTime = int.Parse(DateTime.Now.ToString("HHmmss"));
                        if (nowTime >= brdTime && brdTime + runTime > nowTime)
                        //
                        {
                            item.Now_Playing = true; // 방송중인 프로그램 플래그
                            NowProgram = item;
                            //여기여기 
                        }
                        else
                        {
                            item.Now_Playing = false; // 방송중이지 않은 프로그램 플래그
                        }
                    }
                    Console.WriteLine($"방송중인 프로그램 : {NowProgram.SDDT_TITLE}");
                }
                Thread.Sleep(100); // 100ms 대기

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


    }
}
