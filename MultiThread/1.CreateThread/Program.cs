using System;
using System.Threading;

namespace CreateThread
{
    class Program
    {
        static void Main()
        {
            //*****************************************************************
            // 使用无参数委托 public delegate void ThreadStart();
            // Thread t = new Thread(new ThreadStart(ThreadFunc));
            // Thread t = new Thread(ThreadFunc);
            // Thread t = new Thread ( () => { for (int i = 0; i < 10; i++) Console.Write(Thread.CurrentThread.Name);} );
            Thread t1 = new Thread(new ThreadStart(ThreadFunc)) {Name = "1"};
            Thread t2 = new Thread(ThreadFunc) {Name = "2"};
            Thread t3 = new Thread(() => { for (int i = 0; i < 10; i++) Console.Write(Thread.CurrentThread.Name); })
            {
                Name = "3"
            };
            t1.IsBackground = true; // By default, threads you create explicitly are foreground threads.

            // 依次启动3个线程
            t1.Start();
            t2.Start();
            t3.Start();

            Thread.Sleep(1000);// 等待线程结束
            Console.WriteLine();

            //*****************************************************************
            // 使用带参数委托 public delegate void ParameterizedThreadStart (object obj);
            // Thread t = new Thread(ThreadFuncWithParameter); t.Start("y");
            // Thread t = new Thread ( () => ThreadFuncWithParameter("y") ); t.Start();
            Thread printMsgThread = new Thread(ThreadFuncWithParameter);
            printMsgThread.Start("y");
            
            ThreadFuncWithParameter("x");
            Console.WriteLine("\nPrint Message Thread:IsBackground={0}, Priority={1}",
                printMsgThread.IsBackground ? "true" : "false", printMsgThread.Priority);
            printMsgThread.Join();// 等待线程完成
            Console.WriteLine("\nPrint Message Thread has ended!");

            // Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
        }

        static void ThreadFunc()
        {
            for (int i = 0; i < 10; i++) Console.Write(Thread.CurrentThread.Name);
        }

        static void ThreadFuncWithParameter(object paraObj)
        {
            string message = (string)paraObj;
            for (int i = 0; i < 100; i++) Console.Write(message);

            //Thread.Sleep(TimeSpan.FromHours(1));  // sleep for 1 hour
            Thread.Sleep(500);                     // sleep for 500 milliseconds
        }
    }
}
