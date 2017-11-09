using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Medicom.Concurrent
{
    // 使用Task执行的版本
    public class TaskQueueTaskCompletionSource : ITaskQueue
    {
        private class WorkItem
        {
            public readonly TaskCompletionSource<object> TaskSource;
            public readonly Action Action;
            public readonly CancellationToken? CancelToken;

            public WorkItem(
                TaskCompletionSource<object> taskSource,
                Action action,
                CancellationToken? cancelToken)
            {
                TaskSource = taskSource;
                Action = action;
                CancelToken = cancelToken;
            }
        }

        private readonly BlockingCollection<WorkItem> _taskQueue;
        private readonly Task[] _workers;

        public TaskQueueTaskCompletionSource(int workerCount, int maxSize = 1000)
        {
            _taskQueue = new BlockingCollection<WorkItem>(maxSize);

            _workers = new Task[workerCount];

            // Create and start a separate Task for each consumer:
            for (var i = 0; i < workerCount; i++)
                _workers[i] = Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            Shutdown();
        }

        public void EnqueueTask(Action action)
        {
            EnqueueTask(action, null);
        }

        public Task EnqueueTaskEx(Action action)
        {
            return EnqueueTask(action, null);
        }

        public Task EnqueueTask(Action action, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<object>();
            _taskQueue.Add(new WorkItem(tcs, action, cancelToken));
            return tcs.Task;
        }

        public void Consume()
        {
            foreach (var workItem in _taskQueue.GetConsumingEnumerable())
            {
                if (workItem.CancelToken.HasValue &&
                    workItem.CancelToken.Value.IsCancellationRequested)
                {
                    workItem.TaskSource.SetCanceled();
                }
                else
                {
                    try
                    {
                        workItem.Action();
                        workItem.TaskSource.SetResult(null); // Indicate completion
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == workItem.CancelToken)
                            workItem.TaskSource.SetCanceled();
                        else
                            workItem.TaskSource.SetException(ex);
                    }
                    catch (Exception ex)
                    {
                        workItem.TaskSource.SetException(ex);
                    }
                }
            }
        }

        public void Shutdown()
        {
            _taskQueue.CompleteAdding();

            // Wait for all the tasks to finish.
            Task.WaitAll(_workers);
        }
    }

    // 使用Thread执行的版本
    // Thread的实时性要优于Task
    public class TaskQueueTaskCompletionSourceThread : ITaskQueue
    {
        private class WorkItem
        {
            public readonly TaskCompletionSource<object> TaskSource;
            public readonly Action Action;
            public readonly CancellationToken? CancelToken;

            public WorkItem(
                TaskCompletionSource<object> taskSource,
                Action action,
                CancellationToken? cancelToken)
            {
                TaskSource = taskSource;
                Action = action;
                CancelToken = cancelToken;
            }
        }

        private readonly BlockingCollection<WorkItem> _taskQueue;
        private readonly Thread[] _workers;

        public TaskQueueTaskCompletionSourceThread(int workerCount, int maxSize = 1000)
        {
            _taskQueue = new BlockingCollection<WorkItem>(maxSize);

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
            EnqueueTask(action, null);
        }

        public Task EnqueueTaskEx(Action action)
        {
            return EnqueueTask(action, null);
        }

        public Task EnqueueTask(Action action, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<object>();
            _taskQueue.Add(new WorkItem(tcs, action, cancelToken));
            return tcs.Task;
        }

        public void Consume()
        {
            foreach (var workItem in _taskQueue.GetConsumingEnumerable())
            {
                if (workItem.CancelToken.HasValue &&
                    workItem.CancelToken.Value.IsCancellationRequested)
                {
                    workItem.TaskSource.SetCanceled();
                }
                else
                {
                    try
                    {
                        workItem.Action();
                        workItem.TaskSource.SetResult(null); // Indicate completion
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == workItem.CancelToken)
                            workItem.TaskSource.SetCanceled();
                        else
                            workItem.TaskSource.SetException(ex);
                    }
                    catch (Exception ex)
                    {
                        workItem.TaskSource.SetException(ex);
                    }
                }
            }
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
