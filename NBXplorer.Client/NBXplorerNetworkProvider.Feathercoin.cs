using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitFeathercoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Feathercoin.Instance, networkType)
			{
				MinRPCVersion = 160000,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("8'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetFTC()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Feathercoin.Instance.CryptoCode);
		}
	}
}
