using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
	public partial class NRXplorerNetworkProvider
	{
		private void InitBCash(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.BCash.Instance, networkType)
			{
				MinRPCVersion = 140200,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("145'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetBCH()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.BCash.Instance.CryptoCode);
		}
	}
}
