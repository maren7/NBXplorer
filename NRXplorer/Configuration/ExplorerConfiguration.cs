using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NRealbit;
using System.IO;
using System.Net;
using NRXplorer.Logging;
using NRealbit.Protocol;
using NRealbit.DataEncoders;
using NRealbit.RPC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;

namespace NRXplorer.Configuration
{
	public class ChainConfiguration
	{
		public bool Rescan
		{
			get;
			set;
		}
		public RPCClient RPC
		{
			get;
			internal set;
		}
		public EndPoint NodeEndpoint
		{
			get;
			internal set;
		}
		public int StartHeight
		{
			get;
			internal set;
		}
		public Money MinUtxoValue
		{
			get;
			internal set;
		}
		public string CryptoCode
		{
			get;
			set;
		}
		public bool HasTxIndex { get; set; }
		public bool ExposeRPC { get; set; }
	}
	public class ExplorerConfiguration
	{
		public string ConfigurationFile
		{
			get;
			set;
		}
		public string BaseDataDir
		{
			get;
			set;
		}

		public string DataDir
		{
			get; set;
		}
		public string SignalFilesDir { get; set; }
		public NRXplorerNetworkProvider NetworkProvider
		{
			get; set;
		}
		public RPCArgs RPC
		{
			get;
			set;
		}
		public int MinGapSize
		{
			get; set;
		} = 20;

		public int MaxGapSize
		{
			get; set;
		} = 30;
		public int DBCache { get; set; }
		public List<ChainConfiguration> ChainConfigurations
		{
			get; set;
		} = new List<ChainConfiguration>();

		public ExplorerConfiguration LoadArgs(IConfiguration config)
		{
			NetworkProvider = new NRXplorerNetworkProvider(DefaultConfiguration.GetNetworkType(config));
			var defaultSettings = NRXplorerDefaultSettings.GetDefaultSettings(NetworkProvider.NetworkType);
			BaseDataDir = config.GetOrDefault<string>("datadir", null);
			if(BaseDataDir == null)
			{
				BaseDataDir = Path.GetDirectoryName(defaultSettings.DefaultDataDirectory);
				if(!Directory.Exists(BaseDataDir))
					Directory.CreateDirectory(BaseDataDir);
				if(!Directory.Exists(defaultSettings.DefaultDataDirectory))
					Directory.CreateDirectory(defaultSettings.DefaultDataDirectory);
			}

			Logs.Configuration.LogInformation("Network: " + NetworkProvider.NetworkType.ToString());
			var supportedChains = config.GetOrDefault<string>("chains", "brlb")
									  .Split(',', StringSplitOptions.RemoveEmptyEntries)
									  .Select(t => t.ToUpperInvariant());
			var validChains = new List<string>();
			var exposeRPCGlobal = config.GetOrDefault<bool>("exposerpc", false);
			foreach(var network in NetworkProvider.GetAll())
			{
				if(supportedChains.Contains(network.CryptoCode))
				{
					validChains.Add(network.CryptoCode);
					var chainConfiguration = new ChainConfiguration();
					chainConfiguration.Rescan = config.GetOrDefault<bool>($"{network.CryptoCode}.rescan", false);
					chainConfiguration.CryptoCode = network.CryptoCode;

					var args = RPCArgs.Parse(config, network.NRealbitNetwork, network.CryptoCode);

					chainConfiguration.RPC = args.ConfigureRPCClient(network);
					if (chainConfiguration.RPC.Address.Port == network.NRealbitNetwork.DefaultPort)
					{
						Logs.Configuration.LogWarning($"{network.CryptoCode}: It seems that the RPC port ({chainConfiguration.RPC.Address.Port}) is equal to the default P2P port ({network.NRealbitNetwork.DefaultPort}), this is probably a misconfiguration.");
					}
					if((chainConfiguration.RPC.CredentialString.CookieFile != null || chainConfiguration.RPC.CredentialString.UseDefault) && !network.SupportCookieAuthentication)
					{
						throw new ConfigException($"Chain {network.CryptoCode} does not support cookie file authentication,\n" +
							$"Please use {network.CryptoCode.ToLowerInvariant()}rpcuser and {network.CryptoCode.ToLowerInvariant()}rpcpassword settings in NRXplorer" +
							$"And configure rpcuser and rpcpassword in the configuration file or in commandline or your node");
					}
					chainConfiguration.NodeEndpoint = NRealbit.Utils.ParseEndpoint(config.GetOrDefault<string>($"{network.CryptoCode}.node.endpoint", "127.0.0.1"), network.NRealbitNetwork.DefaultPort);

					if (GetPort(chainConfiguration.NodeEndpoint) == network.NRealbitNetwork.RPCPort)
					{
						Logs.Configuration.LogWarning($"{network.CryptoCode}: It seems that the node endpoint port ({GetPort(chainConfiguration.NodeEndpoint)}) is equal to the default RPC port ({network.NRealbitNetwork.RPCPort}), this is probably a misconfiguration.");
					}

					chainConfiguration.StartHeight = config.GetOrDefault<int>($"{network.CryptoCode}.startheight", -1);

					if (!(network is NRXplorer.NRXplorerNetworkProvider.LiquidNRXplorerNetwork))
					{
						if (config.GetOrDefault<int>($"{network.CryptoCode}.minutxovalue", -1) is int v && v != -1)
						{
							chainConfiguration.MinUtxoValue = Money.Satoshis(v);
						}
					}
					
					chainConfiguration.HasTxIndex = config.GetOrDefault<bool>($"{network.CryptoCode}.hastxindex", false);
					chainConfiguration.ExposeRPC = config.GetOrDefault<bool>($"{network.CryptoCode}.exposerpc", exposeRPCGlobal);
					ChainConfigurations.Add(chainConfiguration);
				}
			}
			var invalidChains = String.Join(',', supportedChains.Where(s => !validChains.Contains(s)).ToArray());
			if(!string.IsNullOrEmpty(invalidChains))
				throw new ConfigException($"Invalid chains {invalidChains} for {NetworkProvider.NetworkType}");

			Logs.Configuration.LogInformation("Supported chains: " + String.Join(',', supportedChains.ToArray()));
			MinGapSize = config.GetOrDefault<int>("mingapsize", 20);
			MaxGapSize = config.GetOrDefault<int>("maxgapsize", 30);
			DBCache = config.GetOrDefault<int>("dbcache", 50);
			if (DBCache > 0)
				Logs.Configuration.LogInformation($"DBCache: {DBCache} MB");
			if (MinGapSize >= MaxGapSize)
				throw new ConfigException("mingapsize should be equal or lower than maxgapsize");
			if(!Directory.Exists(BaseDataDir))
				Directory.CreateDirectory(BaseDataDir);
			DataDir = Path.Combine(BaseDataDir, NRXplorerDefaultSettings.GetFolderName(NetworkProvider.NetworkType));
			if (!Directory.Exists(DataDir))
				Directory.CreateDirectory(DataDir);
			SignalFilesDir = config.GetOrDefault<string>("signalfilesdir", null);
			SignalFilesDir = SignalFilesDir ?? DataDir;
			if (!Directory.Exists(SignalFilesDir))
				Directory.CreateDirectory(SignalFilesDir);
			CacheChain = config.GetOrDefault<bool>("cachechain", true);
			NoAuthentication = config.GetOrDefault<bool>("noauth", false);
			InstanceName = config.GetOrDefault<string>("instancename", "");
			TrimEvents = config.GetOrDefault<int>("trimevents", -1);

			var customKeyPathTemplate = config.GetOrDefault<string>("customkeypathtemplate", null);
			if (!string.IsNullOrEmpty(customKeyPathTemplate))
			{
				if (!KeyPathTemplate.TryParse(customKeyPathTemplate, out var v))
					throw new ConfigException("Invalid customKeyPathTemplate");
				if (v.PostIndexes.IsHardened || v.PreIndexes.IsHardened)
					throw new ConfigException("customKeyPathTemplate should not be an hardened path");
				CustomKeyPathTemplate = v;
			}

			AzureServiceBusConnectionString = config.GetOrDefault<string>("asbcnstr", "");
			AzureServiceBusBlockQueue = config.GetOrDefault<string>("asbblockq", "");
			AzureServiceBusTransactionQueue = config.GetOrDefault<string>("asbtranq", "");
			AzureServiceBusBlockTopic = config.GetOrDefault<string>("asbblockt", "");
			AzureServiceBusTransactionTopic = config.GetOrDefault<string>("asbtrant", "");

			RabbitMqHostName = config.GetOrDefault<string>("rmqhost", "");
			RabbitMqVirtualHost = config.GetOrDefault<string>("rmqvirtual", "");
			RabbitMqUsername = config.GetOrDefault<string>("rmquser", "");
			RabbitMqPassword = config.GetOrDefault<string>("rmqpass", "");
			RabbitMqTransactionExchange = config.GetOrDefault<string>("rmqtranex", "");
			RabbitMqBlockExchange = config.GetOrDefault<string>("rmqblockex", "");

			return this;
		}

		private int GetPort(EndPoint nodeEndpoint)
		{
			if (nodeEndpoint is IPEndPoint endPoint)
				return endPoint.Port;
			else if (nodeEndpoint is DnsEndPoint dnsEndPoint)
				return dnsEndPoint.Port;
			throw new NotSupportedException();
		}

		public bool Supports(NRXplorerNetwork network)
		{
			return ChainConfigurations.Any(c => network.CryptoCode == c.CryptoCode);
		}

		public bool CacheChain
		{
			get;
			set;
		}
		public bool NoAuthentication
		{
			get;
			set;
		}
		public string InstanceName
		{
			get;
			set;
		}
		public int TrimEvents { get; set; }
		public string AzureServiceBusConnectionString
		{
			get;
			set;
		}

		public string AzureServiceBusBlockQueue
		{
			get;
			set;
		}

		public string AzureServiceBusBlockTopic
		{
			get;
			set;
		}

		public string AzureServiceBusTransactionQueue
		{
			get;
			set;
		}
		public string AzureServiceBusTransactionTopic
		{
			get;
			set;
		}

		public string RabbitMqHostName { get; set; }
        public string RabbitMqVirtualHost { get; set; }
        public string RabbitMqUsername { get; set; }
        public string RabbitMqPassword { get; set; }
        public string RabbitMqTransactionExchange { get; set; }
        public string RabbitMqBlockExchange { get; set; }

		public KeyPathTemplate CustomKeyPathTemplate { get; set; }
    }
}
