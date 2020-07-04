using Microsoft.Extensions.Logging;

namespace Open
{
	public abstract class LoggedBase
	{
		protected ILogger Logger { get; }

		protected string TypeName { get; }

		protected LoggedBase(ILogger logger)
		{
			Logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			TypeName = GetType().ToString();
		}
	}
}
