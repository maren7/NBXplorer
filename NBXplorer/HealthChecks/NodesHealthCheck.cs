using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NRXplorer.HealthChecks
{
	public class NodesHealthCheck : IHealthCheck
	{
		public NodesHealthCheck(RealbitDWaiters waiters)
		{
			Waiters = waiters;
		}

		public RealbitDWaiters Waiters { get; }

		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			bool ok = true;
			var data = new Dictionary<string, object>();
			foreach (var waiter in Waiters.All())
			{
				ok &= waiter.RPCAvailable;
				data.Add(waiter.Network.CryptoCode, waiter.State.ToString());
			}
			return Task.FromResult(ok ? HealthCheckResult.Healthy(data: data) : HealthCheckResult.Degraded("Some nodes are not running", data: data));
		}
	}
}
