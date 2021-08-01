using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitQtum(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Qtum.Instance, networkType)
			{
				MinRPCVersion = 140200,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("2301'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetQTUM()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Qtum.Instance.CryptoCode);
		}
	}
}
