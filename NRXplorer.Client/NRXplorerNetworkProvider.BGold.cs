using NRealbit;

namespace NRXplorer
{
    public partial class NRXplorerNetworkProvider
    {
		private void InitBGold(ChainName networkType)
		{
			Add(new NRXplorerNetwork(NRealbit.Altcoins.BGold.Instance, networkType)
			{
				MinRPCVersion = 140200,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("156'") : new KeyPath("1'")
			});
		}

		public NRXplorerNetwork GetBTG()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.BGold.Instance.CryptoCode);
		}
	}
}
