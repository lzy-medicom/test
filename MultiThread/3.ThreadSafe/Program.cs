using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ThreadSafe
{
    class ThreadSafe
    {
        private static bool done;
        static readonly object locker = new object();

        static void Main()
        {
            //*****************************************************************
            // lambda表达式与局部变量
            for (int i = 0; i < 10; i++)
                new Thread(() => Console.Write(i)).Start();

            Thread.Sleep(2000);
            Console.WriteLine("");
            for (int i = 0; i < 10; i++)
            {
                int temp = i;
                new Thread(() => Console.Write(temp)).Start();
            }
            Thread.Sleep(2000);
            Console.WriteLine("\n----------------------------");
            //*****************************************************************
            //
            done = false;
            Thread t = new Thread(Go);
            t.Start();
            Go();

            t.Join();
            Console.WriteLine("----------------------------");

            //*****************************************************************
            //
            done = false;
            new Thread(GoSafe).Start();
            GoSafe();

            //*****************************************************************
            // 测试线程安全的容器
            //long elapsedTicks = DateTime.UtcNow.Ticks;
            Stopwatch sw = Stopwatch.StartNew();
            var d = new ConcurrentDictionary<int, int>();
            for (int i = 0; i < 1000000; i++)
                d[i] = 123;
            sw.Stop();
            Console.WriteLine("ConcurrentDictionary:{0}ms", sw.ElapsedMilliseconds);

            sw.Restart();
            var dd = new Dictionary<int, int>();
            for (int i = 0; i < 1000000; i++)
                lock (dd) dd[i] = 123;

            sw.Stop();
            Console.WriteLine("Dictionary:{0}ms", sw.ElapsedMilliseconds);

            Console.WriteLine("\nPress Enter to exit");
            Console.ReadKey(true);
        }

        static void Go()
        {
            if (!done) { Thread.Sleep(1000); Console.WriteLine("Done"); done = true; }
        }

        static void GoSafe()
        {
            lock (locker)
            {
                if (!done) { Thread.Sleep(1000); Console.WriteLine("Done"); done = true; }
            }
        }
    }
}
