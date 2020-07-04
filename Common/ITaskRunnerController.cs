using System.Threading.Tasks;

namespace Open.TaskManager
{
	public interface ITaskRunnerController
	{
		ValueTask<TaskRunnerState> GetState(int id);

		ValueTask<object?> GetProgress(int id);

		ValueTask<bool> Start(int id);

		ValueTask Stop(int id);
	}
}
