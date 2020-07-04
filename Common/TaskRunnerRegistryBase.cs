using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public abstract class TaskRunnerRegistryBase : LoggedBase, ITaskRunnerRegistry, IAsyncDisposable
	{
		public event Action<int, TaskRunnerState>? StateUpdated;

		public event Action<int, object?>? ProgressUpdated;

		private ConcurrentDictionary<int, ITaskRunner>? _registry
			= new ConcurrentDictionary<int, ITaskRunner>();

		protected ConcurrentDictionary<int, ITaskRunner> Registry
			=> _registry ?? throw new ObjectDisposedException(GetType().ToString());

		protected TaskRunnerRegistryBase(ILogger logger) : base(logger)
		{
		}

		protected void Log(string message, int id) => Logger.LogInformation("TaskRunner ({0}) {1}.", id, message);

		public virtual ValueTask DisposeAsync()
		{
			StateUpdated = null;
			Interlocked.Exchange(ref _registry, null)?.Clear();
			return new ValueTask();
		}

		protected virtual Task SignalStateUpdated(int id, TaskRunnerState state)
		{
			StateUpdated?.Invoke(id, state);
			return Task.CompletedTask;
		}

		protected virtual Task SignalProgressUpdated(int id, object? progress)
		{
			ProgressUpdated?.Invoke(id, progress);
			return Task.CompletedTask;
		}

		public abstract IAsyncEnumerable<int> GetTaskRunnerIds(CancellationToken cancellationToken = default);
		public abstract ValueTask<ITaskRunner> Get(int id);
		public abstract ValueTask<bool> Contains(int id);
		public abstract ValueTask<bool> Remove(int id);
		public abstract ValueTask<TaskRunnerState> GetState(int id);
		public abstract ValueTask<object?> GetProgress(int id);
		public abstract ValueTask<bool> Start(int id);
		public abstract ValueTask Stop(int id);
	}
}
