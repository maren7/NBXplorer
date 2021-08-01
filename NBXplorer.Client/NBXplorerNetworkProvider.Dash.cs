using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitDash(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Dash.Instance, networkType)
			{
				MinRPCVersion = 120000,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("5'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetDASH()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Dash.Instance.CryptoCode);
		}
	}
}
