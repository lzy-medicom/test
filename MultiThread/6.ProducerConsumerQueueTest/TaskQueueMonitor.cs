using System;
using System.Threading;
using System.Collections.Generic;

namespace Medicom.Concurrent
{
    class TaskQueueMonitor : ITaskQueue
    {
        readonly object _locker = new object();
        Thread[] _workers;
        readonly Queue<Action> _taskQueue = new Queue<Action>();

        public TaskQueueMonitor(int workerCount)
        {
            _workers = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i++)
                (_workers[i] = new Thread(Consume)).Start();
        }
        public void Dispose()
        {
            Shutdown();
        }

        public void EnqueueTask(Action action)
        {
            lock (_locker)
            {
                _taskQueue.Enqueue(action);     // We must pulse because we're
                Monitor.Pulse(_locker);         // changing a blocking condition.
            }
        }

        public void Consume()
        {
            while (true)                        // Keep consuming until
            {                                   // told otherwise.
                Action item;
                lock (_locker)
                {
                    while (_taskQueue.Count == 0) Monitor.Wait(_locker);
                    item = _taskQueue.Dequeue();
                }
                if (item == null) return;         // This signals our exit.
                item();                           // Execute item.
            }
        }

        public void Shutdown()
        {
            // Enqueue one null item per worker to make each exit.
            foreach (var worker in _workers)
                EnqueueTask(null);

            // Wait for workers to finish
            //if (waitForWorkers)
            foreach (var worker in _workers)
                worker.Join();
        }
    }
}