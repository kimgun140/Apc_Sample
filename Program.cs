namespace Apc_Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ApcSampleClass apcSampleClass = new ApcSampleClass();

            //Thread Singlethread = new Thread(() =>
            //{
            //    apcSampleClass.ScheduleLoad();
            //    apcSampleClass.ScheduleCheck_Print();
            //});
            //Singlethread.Start();
            // 스레드 한개에서 작업하면 루프에 갖힌다.

            Thread MultiThread1 = new Thread(apcSampleClass.ScheduleLoad);
            MultiThread1.Start();

            Thread MultiThread2 = new Thread(apcSampleClass.ScheduleCheck_Print);
            MultiThread2.Start();
        }
    }
}
