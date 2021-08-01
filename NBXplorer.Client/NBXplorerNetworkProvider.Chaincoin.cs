using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
	public partial class NRXplorerNetworkProvider
	{
		private void InitChaincoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Chaincoin.Instance, networkType)
			{
				MinRPCVersion = 160400,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("711'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetCHC()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Chaincoin.Instance.CryptoCode);
		}
	}
}
