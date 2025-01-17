﻿using NRealbit.RPC;
using NRXplorer.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NRXplorer
{
	public class RPCClientProvider
	{
		Dictionary<string, RPCClient> _ChainConfigurations = new Dictionary<string, RPCClient>();
		public RPCClientProvider(ExplorerConfiguration configuration, IHttpClientFactory httpClientFactory)
		{
			foreach(var config in configuration.ChainConfigurations)
			{
				var rpc = config?.RPC;
				if (rpc != null)
				{
					rpc.HttpClient = httpClientFactory.CreateClient(nameof(RPCClientProvider));
					_ChainConfigurations.Add(config.CryptoCode, rpc);
				}
			}
		}

		public IEnumerable<RPCClient> GetAll()
		{
			return _ChainConfigurations.Values;
		}

		public RPCClient GetRPCClient(string cryptoCode)
		{
			_ChainConfigurations.TryGetValue(cryptoCode, out RPCClient rpc);
			return rpc;
		}
		public RPCClient GetRPCClient(NRXplorerNetwork network)
		{
			return GetRPCClient(network.CryptoCode);
		}
	}
}
