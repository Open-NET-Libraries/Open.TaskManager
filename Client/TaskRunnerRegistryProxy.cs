using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager.Client
{
	public class TaskRunnerRegistryProxy : TaskRunnerRegistryBase
	{
		protected Task<HubConnection> ActiveConnection { get; }

		public TaskRunnerRegistryProxy(
			IHubConnectionFactory hubConnectionFactory,
			ILogger logger) : base(logger)
		{
			var connection = hubConnectionFactory?.Create() ?? throw new ArgumentNullException(nameof(hubConnectionFactory));
			connection.On<int, TaskRunnerState>(nameof(ITaskRunnerClient.StateUpdated), SignalStateUpdated);
			connection.On<int, object?>(nameof(ITaskRunnerClient.ProgressUpdated), SignalProgressUpdated);

			ActiveConnection = Activate();
			async Task<HubConnection> Activate()
			{
				await connection.StartAsync().ConfigureAwait(false);
				return connection;
			}
		}

		public TaskRunnerRegistryProxy(
			IHubConnectionFactory<TaskRunnerRegistryProxy> hubConnectionFactory,
			ILogger<TaskRunnerRegistryProxy> logger) : this(hubConnectionFactory, logger as ILogger)
		{
		}

		public override async ValueTask DisposeAsync()
		{
			await base.DisposeAsync();
			var connection = await ActiveConnection.ConfigureAwait(false);
			await connection.StopAsync().ConfigureAwait(false);
			await connection.DisposeAsync().ConfigureAwait(false);
		}

		public override async ValueTask<ITaskRunner> Get(int id)
		{
			if (Registry.TryGetValue(id, out var runner)) return runner;
			var n = await TaskRunnerProxy.Create(id, this, Logger);
			var r = Registry.GetOrAdd(id, n);
			if (r != n) await n.DisposeAsync();
			else n.StateUpdated.Subscribe(state =>
			{
				if(state==TaskRunnerState.Disposed) Registry.TryRemove(id, out _);
			});
			return r;
		}

		public override async ValueTask<bool> Contains(int id)
			=> await (await ActiveConnection.ConfigureAwait(false)).InvokeAsync<bool>(nameof(Contains), id).ConfigureAwait(false);

		public override async ValueTask<bool> Remove(int id)
			=> await (await ActiveConnection.ConfigureAwait(false)).InvokeAsync<bool>(nameof(Remove), id).ConfigureAwait(false);

		public override async ValueTask<TaskRunnerState> GetState(int id)
			=> await(await ActiveConnection.ConfigureAwait(false)).InvokeAsync<TaskRunnerState>(nameof(GetState), id).ConfigureAwait(false);

		public override async ValueTask<object?> GetProgress(int id)
			=> await(await ActiveConnection.ConfigureAwait(false)).InvokeAsync<object?>(nameof(GetProgress), id).ConfigureAwait(false);

		public override async ValueTask<bool> Start(int id)
			=> await(await ActiveConnection.ConfigureAwait(false)).InvokeAsync<bool>(nameof(Start), id).ConfigureAwait(false);

		public override async ValueTask Stop(int id)
			=> await (await ActiveConnection.ConfigureAwait(false)).InvokeAsync(nameof(Stop), id).ConfigureAwait(false);

		public override async IAsyncEnumerable<int> GetTaskRunnerIds(
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var connection = (await ActiveConnection.ConfigureAwait(false));
			await foreach(var i in connection.StreamAsync<int>(nameof(GetTaskRunnerIds), cancellationToken))
				yield return i;
		}
	}
}
