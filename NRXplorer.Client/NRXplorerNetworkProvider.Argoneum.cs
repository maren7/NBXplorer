using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitArgoneum(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Argoneum.Instance, networkType)
			{
				MinRPCVersion = 1040000,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("421'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetAGM()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Argoneum.Instance.CryptoCode);
		}
	}
}
