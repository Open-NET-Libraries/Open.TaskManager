using System;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public interface ITaskRunner : IAsyncDisposable
	{
		int Id { get; }

		IObservable<TaskRunnerState> StateUpdated { get; }
		IObservable<object?> ProgressUpdated { get; }

		TaskRunnerState State { get; }

		object? Progress { get; }

		ValueTask<bool> Start();

		ValueTask Stop();
	}
}
