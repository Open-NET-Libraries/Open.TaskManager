using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Dispose is properly handled.")]
	public class TaskRunner : TaskRunnerBase
	{
		protected Func<CancellationToken, Action<object?>, Task>? Factory { get; private set; }

		protected TaskRunner(int id, ILogger logger) : base(id, logger)
		{
		}

		public TaskRunner(int id, Func<CancellationToken, Action<object?>, Task> factory, ILogger logger) : this(id, logger)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
			StateSubject.Init(TaskRunnerState.Stopped);
			ProgressSubject.Init(default);
		}

		private readonly object _syncLock = new object();
		private ActiveTask? _active;
		private ActiveTask? _stopping;

		public Task? Active => _active?.Task;

		TaskRunnerState GetState()
		{
			if (_stopping != null) return TaskRunnerState.Stopping;
			if (_active != null) return TaskRunnerState.Running;
			return Factory is null ? TaskRunnerState.Disposed : TaskRunnerState.Stopped;
		}

		bool UpdateState() => UpdateState(GetState());

		public override ValueTask<bool> Start()
		{
			var active = _active;
			if (_active != null) return new ValueTask<bool>(false);

			lock (_syncLock)
			{
				active = _active;
				if (_active != null) return new ValueTask<bool>(false);
				var factory = Factory;
				if (factory is null) throw new ObjectDisposedException(nameof(TaskRunner));
				_active = active = new ActiveTask(
					factory,
					ProgressSubject.OnNext,
					_stopping?.Task,
					() => {
						bool update = true;
						lock (_syncLock)
						{
							if (_stopping == active)
							{
								_stopping = null;
								Log("stopped");
							}
							else if (_active == active)
							{
								_active = null;
								Log("completed");
							}
							else update = false;
						}
						if (update)
						{
							UpdateProgress(null);
							UpdateState();
						}
					});
			}

			UpdateState();
			Log("started");

			return new ValueTask<bool>(true);
		}

		public override async ValueTask Stop()
		{
			var active = _active;
			var stopping = _stopping;
			if (active != null)
			{
				lock (_syncLock)
				{
					active = _active;
					stopping = _stopping;
					if (active != null)
					{
						Log("stopping");
						_stopping = active;
						_active = null;
					}
				}
			}

			if (active is null)
			{
				await Trapped(stopping?.Task);
				return;
			}

			active.Cancel();
			UpdateState();
			// Stopping should not cause any errors.
			await Trapped(active.Task);
		}

		static ValueTask Trapped(Task? task) => task is null ? new ValueTask() : new ValueTask(task.ContinueWith(t => Task.CompletedTask, TaskContinuationOptions.ExecuteSynchronously).Unwrap());

		public override async ValueTask DisposeAsync()
		{
			Log("disposing");
			Factory = null;
			await Stop();
			await base.DisposeAsync();
			Log("disposed");
		}

		class ActiveTask : IDisposable
		{
			public ActiveTask(
				Func<CancellationToken, Action<object?>, Task> factory,
				Action<object?> progressUpdate,
				Task? previous,
				Action onComplete)
			{
				if (factory is null) throw new ArgumentNullException(nameof(factory));
				var canceller = new CancellationTokenSource();
				// Need to guarantee deferred operation to prevent weird state issues.
				Task = Task.Run(() => previous == null
					? factory(canceller.Token, progressUpdate)
					: previous.ContinueWith(t => factory(canceller.Token, progressUpdate), TaskContinuationOptions.ExecuteSynchronously))
					.ContinueWith(t=>
					{
						onComplete();
						return t;
					},
					TaskContinuationOptions.ExecuteSynchronously)
					.Unwrap();

				Task.ContinueWith(t => Dispose());
				_canceller = canceller;
			}

			private CancellationTokenSource? _canceller;
			public Task Task { get; }
			public void Cancel()
			{
				var canceller = Interlocked.Exchange(ref _canceller, null);
				if (canceller is null) return;
				canceller.Cancel();
				canceller.Dispose();
			}
			public void Dispose() => Interlocked.Exchange(ref _canceller, null)?.Dispose();
		}

	}
}
