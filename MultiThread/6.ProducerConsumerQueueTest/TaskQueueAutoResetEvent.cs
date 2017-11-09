using System;
using System.Threading;
using System.Collections.Generic;

namespace Medicom.Concurrent
{
    class TaskQueueAutoResetEvent : ITaskQueue
    {
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread[] _workers;
        readonly object _locker = new object();
        readonly Queue<Action> _taskQueue = new Queue<Action>();
        private bool _isAddingCompleted = false;

        public TaskQueueAutoResetEvent(int workerCount)
        {
            _workers = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (var i = 0; i < workerCount; i++)
            {
                (_workers[i] = new Thread(Consume)).Start();
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        public void EnqueueTask(Action action)
        {
            lock (_locker)
            {
                if (_isAddingCompleted) return;
                _taskQueue.Enqueue(action);
            }
            _wh.Set();
        }

        public void Consume()
        {
            while (true)
            {
                Action action = null;
                lock (_locker)
                    if (_taskQueue.Count > 0)
                    {
                        action = _taskQueue.Dequeue();
                        if (action == null)
                        {
                            return;
                        }
                    }
                if (action != null)
                {
                    action();
                }
                else
                {
                    _wh.WaitOne(); // No more tasks - wait for a signal
                }
            }
        }

        public void Shutdown()
        {
            // Enqueue one null item per worker to make each exit.
            foreach (var worker in _workers)
                EnqueueTask(null);
            
            _isAddingCompleted = true;

            // Wait for workers to finish
            //if (waitForWorkers)
            foreach (var worker in _workers)
                worker.Join();

            _wh.Close();            // Release any OS resources.
        }
    }

    /// <summary>
    /// 和TaskQueueAutoResetEvent的核心代码是一样的，
    /// 只是加大了延时来演示信号丢失
    /// </summary>
    class TaskQueueAutoResetEventLostWakeup : ITaskQueue
    {
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread[] _workers;
        private readonly Dictionary<string, int> _pulseCountDictionary;
        readonly object _locker = new object();
        readonly Queue<Action> _taskQueue = new Queue<Action>();
        private bool _isAddingCompleted = false;

        public TaskQueueAutoResetEventLostWakeup(int workerCount)
        {
            _pulseCountDictionary = new Dictionary<string, int>();
            _workers = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (var i = 0; i < workerCount; i++)
            {
                //(_workers[i] = new Thread(Consume)).Start();
                var threadName = string.Format("ConsumerThread{0}", i);
                _pulseCountDictionary[threadName] = 0;
                _workers[i] = new Thread(Consume) { Name = threadName };
                _workers[i].Start();
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        public void EnqueueTask(Action action)
        {
            lock (_locker)
            {
                if (_isAddingCompleted) return;
                _taskQueue.Enqueue(action);
            }
            _wh.Set();
        }

        public void Consume()
        {
            while (true)
            {
                Action action = null;
                lock (_locker)
                    if (_taskQueue.Count > 0)
                    {
                        action = _taskQueue.Dequeue();
                        if (action == null)
                        {
                            Console.WriteLine("\t[{0}( {1} )]\tthread end(Count of the task queue = {2})",
                                Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId,
                                _taskQueue.Count);
                            return;
                        }
                    }
                if (action != null)
                {
                    action();
                }
                else
                {
                    var threadName = Thread.CurrentThread.Name ?? "ConsumerThread";
                    Console.WriteLine("\t[{0}( {1} )]\tWaitOne...",
                        threadName, Thread.CurrentThread.ManagedThreadId);
                    
                    // 模拟线程切换，即不能立即进入等待状态，这期间的唤醒信号将丢失
                    Thread.Sleep(3000);

                    _wh.WaitOne(); // No more tasks - wait for a signal

                    int pulseCount = _pulseCountDictionary[threadName];
                    pulseCount++;
                    _pulseCountDictionary[threadName] = pulseCount;
                    Console.WriteLine("\t[{0}( {1} )]\tpulseCount={2}",
                        threadName, Thread.CurrentThread.ManagedThreadId,
                        pulseCount);
                }
            }
        }

        public void Shutdown()
        {
            // Enqueue one null item per worker to make each exit.
            foreach (var worker in _workers)
                EnqueueTask(null);

            _isAddingCompleted = true;

            // Wait for workers to finish
            //if (waitForWorkers)
            foreach (var worker in _workers)
                worker.Join();

            _wh.Close();            // Release any OS resources.
        }
    }
}