#define TEST_RUN_TIME

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadAndTask
{
    class Program
    {
        //显示线程现状
        static void ThreadMessage(string data)
        {
            string message = string.Format("\t[ThreadId={1}]\t{0}",
                 data, Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine(message);
        }

        //显示线程池现状
        static void ThreadPoolMessage()
        {
            int a, b;
            ThreadPool.GetAvailableThreads(out a, out b);
            string message = string.Format("\t[ThreadId]={0}\t" +
                "IsBackground:{1}\t" +
                "WorkerThreads is:{2}\tCompletionPortThreads is:{3}\n",
                 Thread.CurrentThread.ManagedThreadId,
                 Thread.CurrentThread.IsBackground.ToString(),
                 a.ToString(), b.ToString());
            Console.WriteLine(message);
        }

        private static void Main(string[] args)
        {
#if (TEST_RUN_TIME)
            // 测试Thread、Task等方式执行线程的效率
            // 
            for (var i = 1; i <= 50; i++)
                TestThreadPool(i);
            for (var i = 1; i <= 50; i++)
                TestTask(i);
            for (var i = 1; i <= 50; i++)
                TestTaskFactory(i);
            for (var i = 1; i <= 50; i++)
                TestThread(i);
            Task.Run(() =>
            {
                Parallel.For(1, 51, i =>
                {
                    Console.WriteLine("Parallel.For {0} start.", i);
                    Thread.Sleep(5000);
                    Console.WriteLine("-------------------Parallel.For {0} end.", i);
                });
            });

    ;
#else            

            #region CLR线程池的工作者线程
            ///////////////////////////////////////////////////////////////////
            // 使用CLR线程池创建工作者线程
            // 1)ThreadPool.QueueUserWorkItem
            //
            Console.WriteLine("\n[Main thread]ThreadPool.QueueUserWorkItem");
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadFunc));
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadFuncWithParameter), "Hello Elva");
            //
            // 通过ThreadPool.QueueUserWorkItem启动工作者线程虽然方便，
            // 但WaitCallback委托指向的必须是一个带有Object参数的无返回值方法，这无疑是一种限制。
            // 若方法需要有返回值，或者带有多个参数，这将多费周折。

            // 2)利用BeginInvoke与EndInvoke完成异步委托方法
            //
            Thread.Sleep(3000);
            Console.WriteLine("\n[Main thread]利用BeginInvoke与EndInvoke完成异步委托方法");
            //建立委托
            MyDelegate myDelegate = new MyDelegate(Hello);
            //异步调用委托，获取计算结果
            IAsyncResult result = myDelegate.BeginInvoke("Leslie", null, null);
            //完成主线程其他工作
            while (!result.AsyncWaitHandle.WaitOne(200))
            {
                Console.WriteLine("  Main thead do work!");
            }
            //等待异步方法完成，调用EndInvoke(IAsyncResult)获取运行结果
            string data = myDelegate.EndInvoke(result);
            ThreadMessage(data);

            // 3)回调函数
            Thread.Sleep(2000);
            Console.WriteLine("\n[Main thread]回调函数");
            //建立委托
            MyDelegate myDelegateCallback = new MyDelegate(Hello);
            //异步调用委托，获取计算结果
            myDelegateCallback.BeginInvoke("Leslie", new AsyncCallback(Completed), null);
            //在启动异步线程后，主线程可以继续工作而不需要等待
            for (int n = 0; n < 6; n++)
            {
                Thread.Sleep(600);
                Console.WriteLine("  Main thread do work!");
            }

            #endregion

            #region CLR线程池的I/O线程

            #endregion

            #region 异步 SqlCommand
            
            #endregion

            #region 并行编程与PLINQ
            ///////////////////////////////////////////////////////////////////
            // 并行编程 http://www.cnblogs.com/leslies2/archive/2012/02/08/2320914.html
            // System.Threading.Tasks中的类被统称为任务并行库（Task Parallel Library，TPL）
            //
            // 1)Func和Action
            // 数据并行Parallel.For 与 Parallel.ForEach

            // 任务并行Parallel.Invoke

            //
            // 2)Task
            Console.WriteLine("\n[Main thread]Task");
            Task.Factory.StartNew(() => ThreadFunc(null));
            ThreadMessage("Main thread do work");

            // 异步（async）和等待（await）
            Thread.Sleep(2000);
            Console.WriteLine("\n[Main thread]异步（async）和等待（await）");
            AsyncMethod();
            ThreadMessage("Main thread do work");

            // 3)并行查询（PLINQ）

            #endregion
#endif

            Thread.Sleep(8000);
            Console.WriteLine("\nPress Enter to exit");
            Console.ReadKey();
        }

        static void ThreadFunc(object state)
        {
            Thread.Sleep(200);
            ThreadMessage("Async thread do work!");
        }
        static void ThreadFuncWithParameter(object state)
        {
            Thread.Sleep(200);
            string data = (string)state;
            ThreadMessage("Async thread with parameter do work, " + data);
        }

        delegate string MyDelegate(string name);

        static string Hello(string name)
        {
            ThreadMessage("Hello Thread");
            Thread.Sleep(2000);            //模拟异步工作
            return "Hello " + name;
        }

        static void Completed(IAsyncResult result)
        {
            ThreadMessage("Async Completed");

            //获取委托对象，调用EndInvoke方法获取运行结果
            AsyncResult _result = (AsyncResult)result;
            MyDelegate myDelegate = (MyDelegate)_result.AsyncDelegate;
            string data = myDelegate.EndInvoke(_result);
            ThreadMessage(data);
        }

        public static async void AsyncMethod()
        {
            await Task.Run(new Action(LongTask));
            
            ThreadMessage("AsyncMethod");
        }

        public static void LongTask()
        {
            Thread.Sleep(5000); 
        }

        #region 测试Thread、Task等方式执行线程的效率

        private static void TestThread(int i)
        {
            Console.WriteLine("Thread {0} start.", i);
            new Thread(h =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("-------------------Thread {0} end.", i);
            }).Start();
        }

        private static void TestThreadPool(int i)
        {
            Console.WriteLine("ThreadPool {0} start.", i);
            ThreadPool.QueueUserWorkItem(h =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("-------------------ThreadPool {0} end.", i);
            });
        }

        private static void TestTask(int i)
        {
            Console.WriteLine("Task {0} start.", i);
            new Task(() =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("-------------------Task {0} end.", i);
            }).Start();
        }

        private static void TestTaskFactory(int i)
        {
            Console.WriteLine("TaskFactory {0} start.", i);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("-------------------TaskFactory {0} end.", i);
            });
        }

        #endregion

    }
}
