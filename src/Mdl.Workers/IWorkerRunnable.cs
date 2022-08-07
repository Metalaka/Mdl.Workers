namespace Mdl.Workers;

public interface IWorkerRunnable : IWorker
{
    void Start();

    void Stop();
}