namespace Mdl.Workers;

public interface IWorkerPausable : IWorkerRunnable
{
    void Pause();

    void Resume();
}