using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public class TaskRunnerRegistry : TaskRunnerRegistryBase, ITaskRunnerRegistryService
	{
		int LatestId;

		public TaskRunnerRegistry(ILogger logger) : base(logger)
		{
		}

		public TaskRunnerRegistry(ILogger<ITaskRunnerRegistryService> logger) : this(logger as ILogger)
		{
		}

		public override ValueTask<ITaskRunner> Get(int id)
			=> new ValueTask<ITaskRunner>(Registry[id]);

		public override ValueTask<bool> Contains(int id)
			=> new ValueTask<bool>(Registry.ContainsKey(id));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentionally avoiding throw.")]
		public override async ValueTask<TaskRunnerState> GetState(int id)
		{
			try
			{
				var runner = await Get(id);
				return runner.State;
			}
			catch (IndexOutOfRangeException) { }
			catch (KeyNotFoundException) { }
			catch (Exception ex) { Logger.LogError(ex, "When requesting state for id [{0}].", id); }
			return TaskRunnerState.Unknown;
		}

		public override async ValueTask<object?> GetProgress(int id)
		{
			var runner = await Get(id);
			return runner.Progress;
		}

		public override async ValueTask<bool> Start(int id)
		{
			var runner = await Get(id);
			return await runner.Start();
		}

		public override async ValueTask Stop(int id)
		{
			var runner = await Get(id);
			await runner.Stop();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposal happens later.")]
		public ValueTask<ITaskRunner> Create(Func<CancellationToken, Action<object?>, Task> factory)
		{
			var registry = Registry;
			var id = Interlocked.Increment(ref LatestId);
			var runner = new TaskRunner(id, factory, Logger);
			if (!Registry.TryAdd(id, runner)) throw new Exception($"Id ({id}) already exists"); // should never happen, but lets cover this case.
			runner.ProgressUpdated.Subscribe(progress => SignalProgressUpdated(id, progress));
			runner.StateUpdated.Subscribe(state => SignalStateUpdated(id, state));
			Log("created", runner.Id);
			return new ValueTask<ITaskRunner>(runner);
		}

		public override async ValueTask<bool> Remove(int id)
		{
			if (!Registry.TryRemove(id, out var runner)) return false;
			Log("removed from regsisty", runner.Id);
			await runner.DisposeAsync();
			return true;
		}

		public override async ValueTask DisposeAsync()
		{
			var disposing = new List<Task>();
			await foreach (var i in GetTaskRunnerIds()) disposing.Add(Remove(i).AsTask());
			await Task.WhenAll(disposing).ConfigureAwait(false);
			await base.DisposeAsync();
		}

		public override async IAsyncEnumerable<int> GetTaskRunnerIds([EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var keys = Registry.Keys.ToArray();
			foreach (var i in keys)
			{
				if (cancellationToken.IsCancellationRequested) break;
				if (await Contains(i)) yield return i;
			}
		}
	}
}
