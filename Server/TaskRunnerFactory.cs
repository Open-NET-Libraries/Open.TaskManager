using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager.Server
{
	public class TaskRunnerFactory : ITaskRunnerFactory
	{
		public ITaskRunnerRegistryService Registry { get; }
		protected TaskRunnerFactoryDelegate Factory { get; }

		public TaskRunnerFactory(ITaskRunnerRegistryService registry, TaskRunnerFactoryDelegate factory)
		{
			Registry = registry ?? throw new ArgumentNullException(nameof(registry));
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public ValueTask<ITaskRunner> CreateTaskRunner() => Registry.Create(Factory);
	}

	public class TaskRunnerFactory<TIdentity> : TaskRunnerFactory, ITaskRunnerFactory<TIdentity>
	{
		public TaskRunnerFactory(ITaskRunnerRegistryService registry, TaskRunnerFactoryDelegate factory) : base(registry, factory)
		{
		}
	}
}
