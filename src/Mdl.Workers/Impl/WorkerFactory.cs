namespace Mdl.Workers.Impl;

using System;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

public static class WorkerFactory
{
    public static IWorkerPausable Create(Action action)
    {
        Guard.IsNotNull(action, nameof(action));

        return new Worker(action);
    }

    public static IWorkerPausable Create(Func<Task> action)
    {
        Guard.IsNotNull(action, nameof(action));

        return new Worker(() => action.Invoke().Wait());
    }
}
