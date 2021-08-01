﻿using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitPolis(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Polis.Instance, networkType)
			{
				MinRPCVersion = 1030000,
				CoinType = NetworkType == ChainName.Mainnet ? new KeyPath("1997'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetPOLIS()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Polis.Instance.CryptoCode);
		}
	}
}
