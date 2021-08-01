using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
	public partial class NRXplorerNetworkProvider
	{
		private void InitMonetaryUnit(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.MonetaryUnit.Instance, networkType)
			{
				MinRPCVersion = 70702
			});
		}

		public NRXplorerNetwork GetMUE()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.MonetaryUnit.Instance.CryptoCode);
		}
	}
}
