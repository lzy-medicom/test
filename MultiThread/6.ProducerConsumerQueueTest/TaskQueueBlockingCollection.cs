using System;
using System.Threading;
using System.Collections.Concurrent;

namespace Medicom.Concurrent
{
    class TaskQueueBlockingCollection : ITaskQueue
    {
        readonly Thread[] _workers;
        readonly BlockingCollection<Action> _taskQueue;
        public TaskQueueBlockingCollection(int workerCount, int maxSize = 1000)
        {
            _taskQueue = new BlockingCollection<Action>(maxSize);

            _workers = new Thread[workerCount];

            // Create and start a separate Task for each consumer:
            for (var i = 0; i < workerCount; i++)
                (_workers[i] = new Thread(Consume)).Start();
        }
        public void Dispose()
        {
            Shutdown();
        }

        public void EnqueueTask(Action action)
        {
            _taskQueue.Add(action);
        }

        public void Consume()
        {
            // This sequence that we’re enumerating will block when no elements
            // are available and will end when CompleteAdding is called. 
            foreach (var action in _taskQueue.GetConsumingEnumerable())
                action();     // Perform task.
        }

        public void Shutdown()
        {
            _taskQueue.CompleteAdding();

            // Wait for workers to finish
            foreach (var worker in _workers)
                worker.Join();
        }
    }
}