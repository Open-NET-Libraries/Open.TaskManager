using System.Threading.Tasks;

namespace Open.TaskManager.Server
{
	public interface ITaskRunnerFactory
	{
		ValueTask<ITaskRunner> CreateTaskRunner();
		ITaskRunnerRegistryService Registry { get; }
	}

	// For use with DI.
	public interface ITaskRunnerFactory<TIdentity> : ITaskRunnerFactory
	{

	}
}
