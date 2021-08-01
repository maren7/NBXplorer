using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitDogecoin(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Dogecoin.Instance, networkType)
			{
				MinRPCVersion = 140200,
				ChainLoadingTimeout = TimeSpan.FromHours(1),
				ChainCacheLoadingTimeout = TimeSpan.FromMinutes(2),
				SupportCookieAuthentication = false,
				CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("3'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetDOGE()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Dogecoin.Instance.CryptoCode);
		}
	}
}
