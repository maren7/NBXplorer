using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitMonacoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Monacoin.Instance, networkType)
			{
				MinRPCVersion = 140200,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("22'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetMONA()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Monacoin.Instance.CryptoCode);
		}
	}
}
