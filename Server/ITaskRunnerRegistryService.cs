using Open.TaskManager.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public interface ITaskRunnerRegistryService : ITaskRunnerRegistry
	{
		ValueTask<ITaskRunner> Create(TaskRunnerFactoryDelegate factory);

		public ITaskRunnerFactory GetFactory(TaskRunnerFactoryDelegate factory)
			=> new TaskRunnerFactory(this, factory);

		public ITaskRunnerFactory<TIdentity> GetFactory<TIdentity>(TaskRunnerFactoryDelegate factory)
			=> new TaskRunnerFactory<TIdentity>(this, factory);
	}
}
