using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
	public partial class NRXplorerNetworkProvider
    {
		private void InitGroestlcoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Groestlcoin.Instance, networkType)
			{
				MinRPCVersion = 2160000,
				CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("17'") : new KeyPath("1'")
			});
		}
	}
}
