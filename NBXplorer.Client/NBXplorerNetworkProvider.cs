using NRealbit;
using System.Collections.Generic;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		public NRXplorerNetworkProvider(ChainName networkType)
		{
			NetworkType = networkType;
			InitArgoneum(networkType);
			InitRealbit(networkType);
			InitBitcore(networkType);
			InitLitecoin(networkType);
			InitDogecoin(networkType);
			InitBCash(networkType);
			InitGroestlcoin(networkType);
			InitBGold(networkType);
			InitDash(networkType);
			InitTerracoin(networkType);
			InitPolis(networkType);
			InitMonacoin(networkType);
			InitFeathercoin(networkType);
			InitUfo(networkType);
			InitViacoin(networkType);
			InitMonoeci(networkType);
			InitGobyte(networkType);
			InitColossus(networkType);
			InitChaincoin(networkType);
			InitLiquid(networkType);
			InitQtum(networkType);
			InitAlthash(networkType);
			InitMonetaryUnit(networkType);
			foreach (var chain in _Networks.Values)
			{
				chain.DerivationStrategyFactory ??= chain.CreateStrategyFactory();
			}
		}

		public ChainName NetworkType
		{
			get;
			private set;
		}

		public NRXplorerNetwork GetFromCryptoCode(string cryptoCode)
		{
			_Networks.TryGetValue(cryptoCode.ToUpperInvariant(), out NRXplorerNetwork network);
			return network;
		}

		public IEnumerable<NRXplorerNetwork> GetAll()
		{
			return _Networks.Values;
		}

		Dictionary<string, NRXplorerNetwork> _Networks = new Dictionary<string, NRXplorerNetwork>();
		private void Add(NRXplorerNetwork network)
		{
			if (network.NRealbitNetwork == null)
				return;
			_Networks.Add(network.CryptoCode, network);
		}
	}
}
