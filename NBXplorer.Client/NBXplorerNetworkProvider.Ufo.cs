using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitUfo(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Ufo.Instance, networkType)
			{
				MinRPCVersion = 150000,
				CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("202'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetUFO()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Ufo.Instance.CryptoCode);
		}
	}
}
