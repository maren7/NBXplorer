using NRealbit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NRXplorer
{
	public partial class NRXplorerNetworkProvider
	{
		private void InitBitcore(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Bitcore.Instance, networkType)
			{
				MinRPCVersion = 80007,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("160'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetBTX()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Bitcore.Instance.CryptoCode);
		}
	}
}
