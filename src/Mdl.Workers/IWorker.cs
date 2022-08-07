namespace Mdl.Workers;

using System;

public interface IWorker : IDisposable
{
    WorkerState State { get; }
}