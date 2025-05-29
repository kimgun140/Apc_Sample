using System.ComponentModel;

namespace Apc_Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ApcSampleClass apcSampleClass = new ApcSampleClass();



            //apcSampleClass.APCEventHandler += apcSampleClass.ScheduleChanging;

            //apcSampleClass.APCEventHandler += apcSampleClass.TirggeredEvent;

            // 이벤트 발생 => 스케줄 로드  
            apcSampleClass.APCEventHandler += apcSampleClass.ScheduleLoad;

            //apcSampleClass.TriggerEvent += () =>
            //{
            //     apcSampleClass.ScheduleLoad();
            //};

            //apcSampleClass.APCEventHandler += apcSampleClass.EventOuccur;

            Thread MultiThread1 = new Thread(() =>
            {
                apcSampleClass.ScheduleLoad();
            });
            MultiThread1.Start();

            Thread MultiThread2 = new Thread(apcSampleClass.ScheduleCheck_Print);
            MultiThread2.Start();



            //apcSampleClass.TirggeredEvent();

            // 커서 스레드가 막히지 않으려면 다른 스레드에서 작업하는게 
            //Thread ChangedScheduleThread = new Thread(apcSampleClass.ScheduleLoad);
            //ChangedScheduleThread.Start();






        }

    }
}
