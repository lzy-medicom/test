using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medicom.Concurrent
{
    public interface ITaskQueue : IDisposable
    {
        void EnqueueTask(Action action);
        void Consume();
        void Shutdown();
    }
}
