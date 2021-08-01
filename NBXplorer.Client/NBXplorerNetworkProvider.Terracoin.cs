using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitTerracoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Terracoin.Instance, networkType)
			{
				MinRPCVersion = 120204,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("83'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetTRC()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Terracoin.Instance.CryptoCode);
		}
	}
}
