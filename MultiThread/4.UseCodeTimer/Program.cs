using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using medicom;

namespace _4.UseCodeTimer
{
    class Program
    {
        #region TestName
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
        #endregion

        static void Main(string[] args)
        {
            // CodeTimer.Initialize方法应该在测试开始前调用
            CodeTimer.Initialize();

            //*****************************************************************
            // 演示Sleep和空循环的差异
            TestName("Sleep and loop");
            CodeTimer.Time("Thread Sleep", 1, () => { Thread.Sleep(3000); });
            CodeTimer.Time("Empty Method", 10000000, () => { });

            //*****************************************************************
            // 演示各种字符串处理方式的执行效率
            //StringPerformanceTests();

            //*****************************************************************
            // 演示多线程环境下锁的效率
            ThreadingPerformanceTests();

            Console.WriteLine("\nPress enter to exit...");
            Console.ReadKey(true);
        }
        
        #region String Performance Tests
        private static void StringPerformanceTests()
        {
            TestName();
            const Int32 iterations = 100 * 1000;

            TestName("String Concatenation Tests");

            String s = String.Empty;
            CodeTimer.Time("Concatenating to a String", iterations,
               () => s += "X");

            StringBuilder sb = new StringBuilder(String.Empty);
            CodeTimer.Time("Concatenating to a StringBuilder", iterations,
               () => sb.Append("X"));

            TestName("String Interning Tests");

            String[] theTypes = TypesFromAssembly();
            String word = "NonExistantType";
            CodeTimer.Time("Lookup without string interning", iterations,
               () =>
               {
                   for (Int32 i = 0; i < theTypes.Length; i++)
                   {
                       if (String.Equals(word, theTypes[i], StringComparison.Ordinal))
                           break;
                   }
               });


            CodeTimer.Time("Time to intern string array", 1,
               () =>
               {
                   word = String.Intern(word);
                   for (int i = 0; i < theTypes.Length; i++)
                       theTypes[i] = String.Intern(theTypes[i]);
               });

            CodeTimer.Time("Lookup with string interning", iterations,
               () =>
               {
                   for (Int32 i = 0; i < theTypes.Length; i++)
                       if (Object.ReferenceEquals(word, theTypes[i]))
                           break;
               });

            TestName("String InterOp Tests");

            CodeTimer.Time("Interop with ANSI String", iterations,
               () => lstrlenAnsi("This is a string"));

            CodeTimer.Time("Interop with UNICODE String", iterations,
               () => lstrlenUnicode("This is a string"));
        }

        private static String[] TypesFromAssembly()
        {
            return (typeof(Object).Assembly.GetTypes().Select(T => T.FullName).ToArray());
        }

        [DllImport("Kernel32", ExactSpelling = true, EntryPoint = "lstrlenA")]
        private static extern Int32 lstrlenAnsi(String s);

        [DllImport("Kernel32", ExactSpelling = true, EntryPoint = "lstrlenW", CharSet = CharSet.Unicode)]
        private static extern Int32 lstrlenUnicode(String s);

        #endregion

        #region Threading Performance Tests

        private static void ThreadingPerformanceTests()
        {
            TestName();
            const Int32 iterations = 1000000;

            TestName("Lock vs. ReaderWriterLock vs. ReaderWriterLockSlim vs. Handle-based");

            Object o = new Object();
            CodeTimer.Time("Lock perf: Monitor", iterations,
               () =>
               {
                   Monitor.Enter(o);
                   Monitor.Exit(o);
               });

            Mutex mutex = new Mutex();
            CodeTimer.Time("Lock perf: Mutex", iterations,
               () =>
               {
                   mutex.WaitOne();
                   mutex.ReleaseMutex();
               });

            var spinLock = new SpinLock(true);
            CodeTimer.Time("Lock perf: SpinLock", iterations,
               () =>
               {
                   bool lockTaken = false;
                   try
                   {
                       spinLock.Enter(ref lockTaken);
                   }
                   finally
                   {
                       if (lockTaken) spinLock.Exit();
                   }
               });

            ReaderWriterLockSlim rwls = new ReaderWriterLockSlim();
            CodeTimer.Time("Lock perf: ReaderWriterLockSlim", iterations,
               () =>
               {
                   rwls.EnterWriteLock();
                   rwls.ExitWriteLock();
               });

            ReaderWriterLock rwl = new ReaderWriterLock();
            CodeTimer.Time("Lock perf: ReaderWriterLock", iterations,
               () =>
               {
                   rwl.AcquireWriterLock(Timeout.Infinite);
                   rwl.ReleaseWriterLock();
               });
        }
        #endregion
    }
}
