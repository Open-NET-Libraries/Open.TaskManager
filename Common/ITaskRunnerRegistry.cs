using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public interface ITaskRunnerRegistry : ITaskRunnerController
	{
		event Action<int, TaskRunnerState>? StateUpdated;

		event Action<int, object?>? ProgressUpdated;

		ValueTask<ITaskRunner> Get(int id);

		ValueTask<bool> Contains(int id);

		ValueTask<bool> Remove(int id);

		IAsyncEnumerable<int> GetTaskRunnerIds(CancellationToken cancellationToken = default);
	}
}
