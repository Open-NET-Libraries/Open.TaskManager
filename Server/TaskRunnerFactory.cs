using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public class TaskRunnerFactory : ITaskRunnerFactory
	{
		public ITaskRunnerRegistryService Registry { get; }
		protected Func<CancellationToken, Action<object?>, Task> Factory { get; }

		public TaskRunnerFactory(ITaskRunnerRegistryService registry, Func<CancellationToken, Action<object?>, Task> factory)
		{
			Registry = registry ?? throw new ArgumentNullException(nameof(registry));
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public ValueTask<ITaskRunner> CreateTaskRunner() => Registry.Create(Factory);
	}

	public class TaskRunnerFactory<TIdentity> : TaskRunnerFactory, ITaskRunnerFactory<TIdentity>
	{
		public TaskRunnerFactory(ITaskRunnerRegistryService registry, Func<CancellationToken, Action<object?>, Task> factory) : base(registry, factory)
		{
		}
	}
}
