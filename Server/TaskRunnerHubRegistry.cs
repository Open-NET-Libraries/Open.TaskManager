using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Open.TaskManager.Server
{
	public class TaskRunnerHubRegistry<THub> : TaskRunnerRegistry
		where THub : Hub<ITaskRunnerClient>
	{
		private readonly IHubContext<THub, ITaskRunnerClient> _context;

		public TaskRunnerHubRegistry(IHubContext<THub, ITaskRunnerClient> context, ILogger<TaskRunnerHubRegistry<THub>> logger) : base(logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		protected override async Task SignalStateUpdated(int id, TaskRunnerState state)
		{
			await base.SignalStateUpdated(id, state).ConfigureAwait(false);
			await _context.Clients.All.StateUpdated(id, state).ConfigureAwait(false);
		}

		protected override async Task SignalProgressUpdated(int id, object? progress)
		{
			await base.SignalProgressUpdated(id, progress).ConfigureAwait(false);
			await _context.Clients.All.ProgressUpdated(id, progress).ConfigureAwait(false);
		}
	}
}
