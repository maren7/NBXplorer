using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitGobyte(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.GoByte.Instance, networkType)
			{
				MinRPCVersion = 120204,
				CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("176'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetGBX()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.GoByte.Instance.CryptoCode);
		}
	}
}
