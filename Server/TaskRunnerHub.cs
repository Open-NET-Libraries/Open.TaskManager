using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager.Server
{
	public class TaskRunnerHub : Hub<ITaskRunnerClient>, ITaskRunnerController
	{
		private ITaskRunnerRegistryService? _registry;
		protected ITaskRunnerRegistryService Registry
			=> _registry ?? throw new ObjectDisposedException(GetType().ToString());

		public TaskRunnerHub(ITaskRunnerRegistryService registry)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
		}

		protected override void Dispose(bool disposing)
		{
			Interlocked.Exchange(ref _registry, null);
			base.Dispose(disposing);
		}

		public IAsyncEnumerable<int> GetTaskRunnerIds(CancellationToken cancellationToken = default) => Registry.GetTaskRunnerIds(cancellationToken);
		public ValueTask<TaskRunnerState> GetState(int id) => Registry.GetState(id);
		public ValueTask<object?> GetProgress(int id) => Registry.GetProgress(id);
		public ValueTask<bool> Start(int id) => Registry.Start(id);
		public ValueTask Stop(int id) => Registry.Stop(id);
	}
}
