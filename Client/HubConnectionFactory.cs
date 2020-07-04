using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Open.TaskManager.Client
{
	public interface IHubConnectionFactory
	{
		HubConnection Create();
	}

	public interface IHubConnectionFactory<TIdentity> : IHubConnectionFactory
	{

	}

	public class HubConnectionFactory<TIdentity> : IHubConnectionFactory<TIdentity>
	{
		private readonly Uri _hubUri;

		public HubConnectionFactory(Uri hubUri)
		{
			_hubUri = hubUri ?? throw new ArgumentNullException(nameof(hubUri));
		}

		public HubConnectionFactory(string hubUrl)
		{
			if (string.IsNullOrWhiteSpace(hubUrl ?? throw new ArgumentNullException(nameof(hubUrl))))
				throw new ArgumentException("Cannot be empty or whitespace.", nameof(hubUrl));
			_hubUri = new Uri(hubUrl);
		}

		public HubConnection Create() => new HubConnectionBuilder()
			.WithUrl(_hubUri)
			.WithAutomaticReconnect()
			.Build();
	}

	public static class HubConnectionFactoryExtensions
	{
		public static IServiceCollection ConfigureHubConnection<TIdentity>(this IServiceCollection services, string hubPath)
			where TIdentity : class
			=> services
			.AddSingleton<IHubConnectionFactory<TIdentity>>(new HubConnectionFactory<TIdentity>(hubPath))
			.AddScoped<TIdentity>();

	}
}
