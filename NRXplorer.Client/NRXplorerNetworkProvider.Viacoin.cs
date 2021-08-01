using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitViacoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Viacoin.Instance, networkType)
			{
				MinRPCVersion = 140200,
				CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("14'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetVIA()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Viacoin.Instance.CryptoCode);
		}
	}
}