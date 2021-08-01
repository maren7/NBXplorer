using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitAlthash(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Althash.Instance, networkType)
			{
				MinRPCVersion = 169900,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("172'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetALTHASH()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Althash.Instance.CryptoCode);
		}
	}
}
