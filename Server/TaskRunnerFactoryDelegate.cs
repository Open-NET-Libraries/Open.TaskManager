using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.TaskManager.Server
{
	public delegate Task TaskRunnerFactoryDelegate(int id, CancellationToken cancellationToken, Action<object?> progress);
}
