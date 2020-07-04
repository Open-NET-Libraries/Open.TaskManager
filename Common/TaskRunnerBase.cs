using Microsoft.Extensions.Logging;
using Open.Observable;
using System;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public abstract class TaskRunnerBase : LoggedBase, ITaskRunner
	{
		public int Id { get; }
		public IObservable<TaskRunnerState> StateUpdated { get; }
		public IObservable<object?> ProgressUpdated { get; }

		protected TaskRunnerBase(int id, ILogger logger) : base(logger)
		{
			if (id < 1) throw new ArgumentOutOfRangeException(nameof(id), id, "Must be greater than zero.");
			Id = id;

			StateSubject = new ObservableValue<TaskRunnerState>();
			ProgressSubject = new ObservableValue<object?>();

			StateUpdated = StateSubject.AsReadOnly();
			ProgressUpdated = ProgressSubject.AsReadOnly();
		}

		protected void Log(string message) => Logger.LogInformation("{0} ({1}) {2}.", TypeName, Id, message);

		protected ObservableValue<TaskRunnerState> StateSubject { get; }
		protected ObservableValue<object?> ProgressSubject { get; }

		public TaskRunnerState State => StateSubject.Value;
		public object? Progress => ProgressSubject.Value;

		protected bool UpdateState(TaskRunnerState state) => StateSubject.Post(state, true);

		protected bool UpdateProgress(object? progress) => ProgressSubject.Post(progress);

		public virtual ValueTask DisposeAsync()
		{
			UpdateState(TaskRunnerState.Disposed);
			StateSubject.Dispose();
			ProgressSubject.Dispose();
			return new ValueTask();
		}

		public abstract ValueTask<bool> Start();

		public abstract ValueTask Stop();
	}
}
