using System;
using System.Collections.Concurrent;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using medicom;
using Medicom.Concurrent;

namespace _6.ProducerConsumerQueueTest
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

            #region 演示采用不同技术实现生产者/消费者队列（ProducerConsumerQueue）
            using (var test = new WriteLineTaskTest())
            {
                const int workerCount = 3;

                TestName("TaskQueueAutoResetEvent");
                test.SetProducerConsumerQueue(new TaskQueueAutoResetEvent(workerCount));
                test.Run();

                TestName("TaskQueueMonitor");
                test.SetProducerConsumerQueue(new TaskQueueMonitor(workerCount));
                test.Run();

                TestName("TaskQueueBlockingCollection");
                test.SetProducerConsumerQueue(new TaskQueueBlockingCollection(workerCount));
                test.Run();

                TestName("TaskQueueTaskCompletionSource");
                test.SetProducerConsumerQueue(new TaskQueueTaskCompletionSource(workerCount));
                test.Run();

                //TestName("TaskQueueAutoResetEventLostWakeup（演示丢失唤醒信号的情况）");
                //test.SetProducerConsumerQueue(new TaskQueueAutoResetEventLostWakeup(workerCount * 2));
                //Thread.Sleep(4000);
                //test.Run();
            }
            #endregion

            //Console.WriteLine("\n演示Task的撤销功能");
            //ControlTaskViaTaskClass();

            //Console.WriteLine("");
            //UsingBlockingCollectionToCommunicateBetweenThreads();

            Console.WriteLine("\nPress enter to exit...");
            Console.ReadKey(true);
        }

        private static void ControlTaskViaTaskClass()
        {
            Console.WriteLine("Enqueuing 10 items...");

            const int workerCount = 3;
            int taskSum = 0;
            //            using (var taskQueue = new TaskQueueTaskCompletionSource(workerCount))
            using (var taskQueue = new TaskQueueTaskCompletionSourceThread(workerCount))
            {
                for (var i = 0; i < 10; i++)
                {
                    var taskIndex = i; // To avoid the captured variable trap
                    Task task;
                    if ((i % 3) == 1)
                    {
                        var tokenSource = new CancellationTokenSource();
                        var token = tokenSource.Token;
                        task = taskQueue.EnqueueTask(() =>
                        {
                            taskSum++;
                            Thread.Sleep(100); // Simulate time-consuming work
                            var threadName = Thread.CurrentThread.Name ?? "ConsumerThread";
                            var message = string.Format("\t[{0}( {1} )]\tTask{2}",
                                threadName, Thread.CurrentThread.ManagedThreadId,
                                taskIndex);
                            Console.WriteLine(message);
                        }, token);
                        token.Register(() =>
                        {
                            Console.WriteLine("\t[{0}( {1} )]\tTask{2}...Canceled",
                                Thread.CurrentThread.Name ?? "ConsumerThread",
                                Thread.CurrentThread.ManagedThreadId,
                                taskIndex);
                        });
                        tokenSource.Cancel();
                    }
                    else
                    {
                        task = taskQueue.EnqueueTaskEx(() =>
                        {
                            taskSum++;
                            Thread.Sleep(100); // Simulate time-consuming work
                            var threadName = Thread.CurrentThread.Name ?? "ConsumerThread";
                            var message = string.Format("\t[{0}( {1} )]\tTask{2}",
                                threadName, Thread.CurrentThread.ManagedThreadId,
                                taskIndex);
                            Console.WriteLine(message);
                        });
                    }
                    Console.WriteLine("[main thread]task index = {0}, TaskId {1}",
                        taskIndex, task.Id);
                    var result = task.ContinueWith<int>((p) =>
                    {
                        Console.WriteLine("TaskId {0} finished!", p.Id);
                        return taskSum;
                    });
                    Thread.Sleep(100);
                }
                Thread.Sleep(100);
                taskQueue.Shutdown();
            }

            Console.WriteLine("Workers complete!({0})", taskSum);
        }

        #region Using BlockingCollection To Communicate Between Threads
        // http://mikehadlow.blogspot.com/2012/11/using-blockingcollection-to-communicate.html
        public class WorkItem
        {
            public string Text { get; set; }
        }
        public static BlockingCollection<WorkItem> ventilatorQueue = new BlockingCollection<WorkItem>();
        public static BlockingCollection<WorkItem> sinkQueue = new BlockingCollection<WorkItem>();
        public static void StartVentilator(BlockingCollection<WorkItem> ventilatorQueue)
        {
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    ventilatorQueue.Add(new WorkItem { Text = string.Format("Item {0}", i) });
                }
            }, TaskCreationOptions.LongRunning);
        }
        public static void StartWorker(int workerNumber,
            BlockingCollection<WorkItem> ventilatorQueue,
            BlockingCollection<WorkItem> sinkQueue)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var workItem in ventilatorQueue.GetConsumingEnumerable())
                {
                    // pretend to take some time to process
                    Thread.Sleep(30);
                    workItem.Text = workItem.Text + " processed by worker " + workerNumber;
                    sinkQueue.Add(workItem);
                }
            }, TaskCreationOptions.LongRunning);
        }
        public static void StartSink(BlockingCollection<WorkItem> sinkQueue)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var workItem in sinkQueue.GetConsumingEnumerable())
                {
                    Console.WriteLine("Processed Messsage: {0}", workItem.Text);
                }
            }, TaskCreationOptions.LongRunning);
        }
        private static void UsingBlockingCollectionToCommunicateBetweenThreads()
        {
            StartSink(sinkQueue);

            StartWorker(0, ventilatorQueue, sinkQueue);
            StartWorker(1, ventilatorQueue, sinkQueue);
            StartWorker(2, ventilatorQueue, sinkQueue);

            StartVentilator(ventilatorQueue);
        }

        #endregion

    }

    class WriteLineTaskTest : IDisposable
    {
        private ITaskQueue _taskQueue = null;

        public void SetProducerConsumerQueue(ITaskQueue taskQueue)
        {
            if (this._taskQueue != null)
            {
                this._taskQueue.Dispose();
            }
            this._taskQueue = taskQueue;
        }

        public void Run()
        {
            Console.WriteLine("Enqueuing 10 items...");

            for (var i = 0; i < 10; i++)
            {
                var taskNumber = i;      // To avoid the captured variable trap
                _taskQueue.EnqueueTask(() =>
                {
                    Thread.Sleep(100);          // Simulate time-consuming work
                    var threadName = Thread.CurrentThread.Name ?? "ConsumerThread";
                    var message = string.Format("\t[{0}( {1} )]\tTask{2}",
                        threadName, Thread.CurrentThread.ManagedThreadId,
                        taskNumber);
                    Console.WriteLine(message);
                });
                Thread.Sleep(100);
            }
            Thread.Sleep(100);
            _taskQueue.Shutdown();
            Console.WriteLine("Workers complete!");
        }

        public void Dispose()
        {
            _taskQueue.Dispose();
        }
    }
}
