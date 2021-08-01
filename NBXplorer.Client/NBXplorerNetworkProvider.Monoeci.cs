using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitMonoeci(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Monoeci.Instance, networkType)
			{
				MinRPCVersion = 120203,
				CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("1998'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetXMCC()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Monoeci.Instance.CryptoCode);
		}
	}
}
