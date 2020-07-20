using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public class TaskRunnerProxy : TaskRunnerBase
	{
		private ITaskRunnerRegistry? _registry;

		protected TaskRunnerProxy(int id, ITaskRunnerRegistry registry, ILogger logger)
			: base(id, logger)
		{
			_registry = registry ?? throw new ArgumentNullException(nameof(registry));
			_registry.StateUpdated += Registry_StateUpdate;
			_registry.ProgressUpdated += Registry_ProgressUpdated;
		}

		public static async ValueTask<TaskRunnerProxy> Create(int id, ITaskRunnerRegistry registry, ILogger logger)
		{
			var trp = new TaskRunnerProxy(id, registry, logger);
			var state = registry.GetState(id);
			var progress = registry.GetProgress(id);
			trp.StateSubject.Init(await state.ConfigureAwait(false));
			trp.ProgressSubject.Init(await progress.ConfigureAwait(false));
			return trp;
		}

		private void Registry_StateUpdate(int id, TaskRunnerState state)
		{
			if (id == Id) UpdateState(state);
		}

		private void Registry_ProgressUpdated(int id, object? progress)
		{
			if (id == Id) UpdateProgress(progress);
		}

		public override async ValueTask DisposeAsync()
		{
			var registry = Interlocked.Exchange(ref _registry, null);
			if (registry != null)
			{
				registry.StateUpdated -= Registry_StateUpdate;
				registry.ProgressUpdated -= Registry_ProgressUpdated;
			}
			await base.DisposeAsync().ConfigureAwait(false);
		}

		public override ValueTask<bool> Start()
			=> _registry?.Start(Id) ?? throw new ObjectDisposedException(TypeName);

		public override ValueTask Stop()
			=> _registry?.Stop(Id) ?? throw new ObjectDisposedException(TypeName);

	}
}
