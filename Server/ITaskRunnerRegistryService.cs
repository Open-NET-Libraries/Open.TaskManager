using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager
{
	public interface ITaskRunnerRegistryService : ITaskRunnerRegistry
	{
		ValueTask<ITaskRunner> Create(Func<CancellationToken, Action<object?>, Task> factory);

		public ITaskRunnerFactory GetFactory(Func<CancellationToken, Action<object?>, Task> factory)
			=> new TaskRunnerFactory(this, factory);

		public ITaskRunnerFactory<TIdentity> GetFactory<TIdentity>(Func<CancellationToken, Action<object?>, Task> factory)
			=> new TaskRunnerFactory<TIdentity>(this, factory);
	}
}
