using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using medicom;

// .NET4.0中使用4.5中的 async/await 功能实现异步
// http://blog.csdn.net/qiujuer/article/details/38511821
// Install-Package Microsoft.Bcl.Async

namespace _7.AsyncAndAwaitDemo
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

            //task->async异步方法和await，主线程碰到await时会立即返回，继续以非阻塞形式执行主线程下面的逻辑  
            Console.WriteLine("---------------------------------");
            Console.WriteLine("①我是主线程，线程ID：{0}", Thread.CurrentThread.ManagedThreadId);
            var testResult = TestAsync(); 

            Console.WriteLine("\nPress enter to exit...");
            Console.ReadKey(true);
        }

        static async Task TestAsync()
        {
            Console.WriteLine("②调用GetReturnResult()之前，线程ID：{0}。当前时间：{1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("yyyy-MM-dd hh:MM:ss"));
            var name = GetReturnResult();
            Console.WriteLine("④调用GetReturnResult()之后，线程ID：{0}。当前时间：{1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("yyyy-MM-dd hh:MM:ss"));
            Console.WriteLine("⑥得到GetReturnResult()方法的结果一：{0}。当前时间：{1}", await name, DateTime.Now.ToString("yyyy-MM-dd hh:MM:ss"));
            Console.WriteLine("⑥得到GetReturnResult()方法的结果二：{0}。当前时间：{1}", name.GetAwaiter().GetResult(), DateTime.Now.ToString("yyyy-MM-dd hh:MM:ss"));
        }

        static async Task<string> GetReturnResult()
        {
            Console.WriteLine("③执行Task.Run之前, 线程ID：{0}", Thread.CurrentThread.ManagedThreadId);
            return await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                Console.WriteLine("⑤GetReturnResult()方法里面线程ID: {0}", Thread.CurrentThread.ManagedThreadId);
                return "我是返回值";
            });
        }
    }
}
