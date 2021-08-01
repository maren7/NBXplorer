using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitColossus(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.Colossus.Instance, networkType)
			{
				MinRPCVersion = 1010000
			});
		}

		public NRXplorerNetwork GetCOLX()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Colossus.Instance.CryptoCode);
		}
	}
}
