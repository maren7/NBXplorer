using NRealbit;
using NRXplorer.DerivationStrategy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NRXplorer
{
	public class NRXplorerNetwork
	{
		internal NRXplorerNetwork(INetworkSet networkSet, ChainName networkType)
		{
			NRealbitNetwork = networkSet.GetNetwork(networkType);
			CryptoCode = networkSet.CryptoCode;
			DefaultSettings = NRXplorerDefaultSettings.GetDefaultSettings(networkType);
		}
		public Network NRealbitNetwork
		{
			get;
			private set;
		}
		
		public int MinRPCVersion
		{
			get;
			internal set;
		}
		public string CryptoCode
		{
			get;
			private set;
		}
		public NRXplorerDefaultSettings DefaultSettings
		{
			get;
			private set;
		}

		internal virtual DerivationStrategyFactory CreateStrategyFactory()
		{
			return new DerivationStrategy.DerivationStrategyFactory(NRealbitNetwork);
		}

		public DerivationStrategy.DerivationStrategyFactory DerivationStrategyFactory
		{
			get;
			internal set;
		}

		public virtual RealbitAddress CreateAddress(DerivationStrategyBase derivationStrategy, KeyPath keyPath, Script scriptPubKey)
		{
			return scriptPubKey.GetDestinationAddress(NRealbitNetwork);
		}

		public bool SupportCookieAuthentication
		{
			get;
			internal set;
		} = true;


		private Serializer _Serializer;
		public Serializer Serializer
		{
			get
			{
				_Serializer = _Serializer ?? new Serializer(this);
				return _Serializer;
			}
		}


		public JsonSerializerSettings JsonSerializerSettings
		{
			get
			{
				return Serializer.Settings;
			}
		}

		

		public TimeSpan ChainLoadingTimeout
		{
			get;
			set;
		} = TimeSpan.FromMinutes(15);

		public TimeSpan ChainCacheLoadingTimeout
		{
			get;
			set;
		} = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Minimum blocks to keep if pruning is activated
		/// </summary>
		public int MinBlocksToKeep
		{
			get; set;
		} = 288;
		public KeyPath CoinType { get; internal set; }

		public override string ToString()
		{
			return CryptoCode.ToString();
		}
		
		public virtual ExplorerClient CreateExplorerClient(Uri uri)
		{
			return new ExplorerClient(this, uri);
		}
	}
}
