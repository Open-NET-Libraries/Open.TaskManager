using System.Threading.Tasks;

namespace Open.TaskManager
{
	public interface ITaskRunnerClient
	{
		Task StateUpdated(int id, TaskRunnerState state);
		Task ProgressUpdated(int id, object? progress);
	}
}
