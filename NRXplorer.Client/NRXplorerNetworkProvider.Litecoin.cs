using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitLitecoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Litecoin.Instance, networkType)
			{
				MinRPCVersion = 140200,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("2'") : new KeyPath("1'"),
			});
		}

		public NRXplorerNetwork GetLTC()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Litecoin.Instance.CryptoCode);
		}
	}
}
