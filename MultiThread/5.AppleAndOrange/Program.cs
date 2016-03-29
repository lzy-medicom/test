using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using medicom;

namespace _5.AppleAndOrange
{
    class Program
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        private static void TestName()
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            StringBuilder testName = new StringBuilder("****************************************\n");
            testName.Append(frames[1].GetMethod().Name);
            testName.Replace('_', ':');
            for (Int32 index = 1; index < testName.Length; index++)
            {
                // If this is an uppercase character, insert a space before it
                if (Char.IsUpper(testName[index]))
                {
                    testName.Insert(index, ' ');
                    index++;
                }
            }
            testName.Append("\n****************************************");
            TestName(testName.ToString());
        }

        private static void TestName(String testName)
        {
            Console.WriteLine();
            Console.WriteLine(testName);
        }

        static void Main(string[] args)
        {
            CodeTimer.Initialize();

            //
            ThreadingPerformanceTests();

            Console.WriteLine("\nPress Enter to exit");
            Console.ReadKey();
        }

        #region 利用多核多线程进行程序优化

        private static void ThreadingPerformanceTests()
        {
            TestName();
            const Int32 iterations = 1;
            //
            //TestName("单线程");
            CodeTimer.Time("单线程", iterations,
               () =>
               {
                   Apple apple = new Apple();
                   Orange orange = new Orange();
                   ulong sum, index;

                   for (sum = 0; sum < Apple.APPLE_MAX_VALUE; sum++)
                   {
                       apple.a += sum;
                       apple.b += sum;
                   }

                   sum = 0;
                   for (index = 0; index < Orange.ORANGE_MAX_VALUE; index++)
                   {
                       sum += orange.a[index] + orange.b[index];
                   }
                   //Console.WriteLine("apple.a={0}, sum={1}", apple.a, sum);
               });

            CodeTimer.Time("两线程", iterations,
               () =>
               {
                   Apple apple = new Apple();
                   Orange orange = new Orange();

                   Thread t = new Thread(
                       () =>
                       {
                           Apple a = apple;
                           ulong s;
                           for (s = 0; s < Apple.APPLE_MAX_VALUE; s++)
                           {
                               a.a += s;
                               a.b += s;
                           }
                       });
                   t.Start();
                   ulong sum = 0;
                   for (ulong index = 0; index < Orange.ORANGE_MAX_VALUE; index++)
                   {
                       sum += orange.a[index] + orange.b[index];
                   }

                   t.Join();
                   //Console.WriteLine("apple.a={0}, sum={1}", apple.a, sum);
               });

            CodeTimer.Time("三线程,加锁", iterations,
               () =>
               {
                   object _locker = new object();
                   Apple apple = new Apple();
                   Orange orange = new Orange();

                   Thread ta = new Thread(
                       () =>
                       {
                           Apple a = apple;
                           ulong s;
                           lock (_locker)
                           {
                               for (s = 0; s < Apple.APPLE_MAX_VALUE; s++)
                               {
                                   a.a += s;
                               }
                           }
                       });
                   Thread tb = new Thread(
                       () =>
                       {
                           Apple a = apple;
                           ulong s;
                           lock (_locker)
                           {
                               for (s = 0; s < Apple.APPLE_MAX_VALUE; s++)
                               {
                                   a.b += s;
                               }
                           }
                       });
                   ta.Start();
                   tb.Start();
                   ulong sum = 0;
                   for (ulong index = 0; index < Orange.ORANGE_MAX_VALUE; index++)
                   {
                       sum += orange.a[index] + orange.b[index];
                   }

                   ta.Join();
                   tb.Join();
                   //Console.WriteLine("apple.a={0}, sum={1}", apple.a, sum);
               });

            CodeTimer.Time("三线程,不加锁", iterations,
               () =>
               {
                   Apple apple = new Apple();
                   Orange orange = new Orange();

                   Thread ta = new Thread(
                       () =>
                       {
                           Apple a = apple;
                           ulong s;
                           for (s = 0; s < Apple.APPLE_MAX_VALUE; s++)
                           {
                               a.a += s;
                           }
                       });
                   Thread tb = new Thread(
                       () =>
                       {
                           Apple a = apple;
                           ulong s;
                           for (s = 0; s < Apple.APPLE_MAX_VALUE; s++)
                           {
                               a.b += s;
                           }
                       });
                   ta.Start();
                   tb.Start();
                   ulong sum = 0;
                   for (ulong index = 0; index < Orange.ORANGE_MAX_VALUE; index++)
                   {
                       sum += orange.a[index] + orange.b[index];
                   }

                   ta.Join();
                   tb.Join();
                   //Console.WriteLine("apple.a={0}, sum={1}", apple.a, sum);
               });

            CodeTimer.Time("三线程,不加锁,Cache:32", iterations,
               () =>
               {
                   AppleWithCache32 apple = new AppleWithCache32();
                   Orange orange = new Orange();

                   Thread ta = new Thread(
                       () =>
                       {
                           AppleWithCache32 a = apple;
                           ulong s;
                           for (s = 0; s < AppleWithCache32.APPLE_MAX_VALUE; s++)
                           {
                               a.a += s;
                           }
                       });
                   Thread tb = new Thread(
                       () =>
                       {
                           AppleWithCache32 a = apple;
                           ulong s;
                           for (s = 0; s < AppleWithCache32.APPLE_MAX_VALUE; s++)
                           {
                               a.b += s;
                           }
                       });
                   ta.Start();
                   tb.Start();
                   ulong sum = 0;
                   for (ulong index = 0; index < Orange.ORANGE_MAX_VALUE; index++)
                   {
                       sum += orange.a[index] + orange.b[index];
                   }

                   ta.Join();
                   tb.Join();
                   //Console.WriteLine("apple.a={0}, sum={1}", apple.a, sum);
               });

            CodeTimer.Time("三线程,不加锁,Cache:64", iterations,
               () =>
               {
                   AppleWithCache64 apple = new AppleWithCache64();
                   Orange orange = new Orange();

                   Thread ta = new Thread(
                       () =>
                       {
                           AppleWithCache64 a = apple;
                           ulong s;
                           for (s = 0; s < AppleWithCache64.APPLE_MAX_VALUE; s++)
                           {
                               a.a += s;
                           }
                       });
                   Thread tb = new Thread(
                       () =>
                       {
                           AppleWithCache64 a = apple;
                           ulong s;
                           for (s = 0; s < AppleWithCache64.APPLE_MAX_VALUE; s++)
                           {
                               a.b += s;
                           }
                       });
                   ta.Start();
                   tb.Start();
                   ulong sum = 0;
                   for (ulong index = 0; index < Orange.ORANGE_MAX_VALUE; index++)
                   {
                       sum += orange.a[index] + orange.b[index];
                   }

                   ta.Join();
                   tb.Join();
                   //Console.WriteLine("apple.a={0}, sum={1}", apple.a, sum);
               });

            CodeTimer.Time("三线程,不加锁,Cache:128", iterations,
               () =>
               {
                   AppleWithCache128 apple = new AppleWithCache128();
                   Orange orange = new Orange();

                   Thread ta = new Thread(
                       () =>
                       {
                           AppleWithCache128 a = apple;
                           ulong s;
                           for (s = 0; s < AppleWithCache128.APPLE_MAX_VALUE; s++)
                           {
                               a.a += s;
                           }
                       });
                   Thread tb = new Thread(
                       () =>
                       {
                           AppleWithCache128 a = apple;
                           ulong s;
                           for (s = 0; s < AppleWithCache128.APPLE_MAX_VALUE; s++)
                           {
                               a.b += s;
                           }
                       });
                   ta.Start();
                   tb.Start();
                   ulong sum = 0;
                   for (ulong index = 0; index < Orange.ORANGE_MAX_VALUE; index++)
                   {
                       sum += orange.a[index] + orange.b[index];
                   }

                   ta.Join();
                   tb.Join();
                   //Console.WriteLine("apple.a={0}, sum={1}", apple.a, sum);
               });
        }

        #endregion
    }

    class Apple
    {
        public UInt64 a = 0;
        public UInt64 b = 0;
        public static uint APPLE_MAX_VALUE = 1000000000;
    }

    class Orange
    {
        public static uint ORANGE_MAX_VALUE = 10000000;
        public uint[] a = new uint[ORANGE_MAX_VALUE];
        public uint[] b = new uint[ORANGE_MAX_VALUE];

        public Orange()
        {
            for (int i = 0; i < ORANGE_MAX_VALUE; i++)
            {
                a[i] = b[i] = (uint)i;
            }
        }
    }

    class AppleWithCache32
    {
        public UInt64 a = 0;
        private byte[] space = new byte[32]; // 32,64,128
        public UInt64 b = 0;
        public static uint APPLE_MAX_VALUE = 100000000;
    }
    class AppleWithCache64
    {
        public UInt64 a = 0;
        private byte[] space = new byte[64]; // 32,64,128
        public UInt64 b = 0;
        public static uint APPLE_MAX_VALUE = 100000000;
    }
    class AppleWithCache128
    {
        public UInt64 a = 0;
        private byte[] space = new byte[128]; // 32,64,128
        public UInt64 b = 0;
        public static uint APPLE_MAX_VALUE = 100000000;
    }
}
