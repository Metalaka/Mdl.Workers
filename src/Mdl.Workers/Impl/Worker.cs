namespace Mdl.Workers.Impl;

using System;
using System.Threading;

internal sealed class Worker : IWorkerRunnable, IWorkerPausable
{
    private const int TimeoutZero = 0;

    private readonly ManualResetEvent _shutdownEvent;
    private readonly ManualResetEvent _pauseEvent;

    private readonly Action _action;
    private readonly Thread _thread;

    internal Worker(Action action)
    {
        _shutdownEvent = new ManualResetEvent(false);
        _pauseEvent = new ManualResetEvent(true);
        _action = action;
        _thread = new Thread(WorkerLoop)
        {
            Name = $"{GetType().FullName} Thread",
        };
    }

    private void WorkerLoop()
    {
        while (true)
        {
            _pauseEvent.WaitOne(Timeout.Infinite);

            if (_shutdownEvent.WaitOne(TimeoutZero))
            {
                break;
            }

            _action.Invoke();
        }
    }

    #region Implementation of IWorkerRunnable

    public void Start()
    {
        if (_thread.ThreadState != ThreadState.Unstarted)
        {
            throw new InvalidOperationException("Worker is already started. Only one invocation of `Start` is allowed. Please see `Pause` method.");
        }

        _thread.Start();
    }

    public void Stop()
    {
        // Signal the shutdown event
        _shutdownEvent.Set();

        // Make sure to resume any paused threads
        _pauseEvent.Set();

        // Wait for the thread to exit
        _thread.Join();
    }

    #endregion

    #region Implementation of IWorkerPausable

    public void Pause()
    {
        _pauseEvent.Reset();
    }

    public void Resume()
    {
        _pauseEvent.Set();
    }

    #endregion

    #region Implementation of IWorker

    public WorkerState State
    {
        get
        {
            if (!_thread.IsAlive || _shutdownEvent.WaitOne(TimeoutZero))
            {
                return WorkerState.Stopped;
            }

            if (_pauseEvent.WaitOne(TimeoutZero))
            {
                return WorkerState.Paused;
            }

            return WorkerState.Running;
        }
    }

    #endregion

    #region Implementation of IDisposable

    public void Dispose()
    {
        Stop();

        _pauseEvent.Dispose();
        _shutdownEvent.Dispose();
    }

    #endregion
}