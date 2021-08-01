using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitRealbit(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Realbit.Instance, networkType)
			{
				MinRPCVersion = 150000,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("0'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetBRLB()
		{
			return GetFromCryptoCode(NRealbit.Realbit.Instance.CryptoCode);
		}
	}
}
