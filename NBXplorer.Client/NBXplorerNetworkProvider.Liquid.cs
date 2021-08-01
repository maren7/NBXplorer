using NRealbit;
using System;
using System.Threading;
using System.Threading.Tasks;
using NRealbit.Altcoins.Elements;
using NRXplorer.DerivationStrategy;
using NRXplorer.Models;

namespace NRXplorer
{
	public partial class NRXplorerNetworkProvider
	{
		public class LiquidNRXplorerNetwork : NRXplorerNetwork
		{
			internal LiquidNRXplorerNetwork(INetworkSet networkSet, ChainName networkType) : base(networkSet, networkType)
			{
			}

			internal override DerivationStrategyFactory CreateStrategyFactory()
			{
				var factory = base.CreateStrategyFactory();
				factory.AuthorizedOptions.Add("unblinded");
				return factory;
			}

			public override RealbitAddress CreateAddress(DerivationStrategyBase derivationStrategy, KeyPath keyPath, Script scriptPubKey)
			{
				if (derivationStrategy.Unblinded())
				{
					return base.CreateAddress(derivationStrategy, keyPath, scriptPubKey);
				}
				var blindingPubKey = GenerateBlindingKey(derivationStrategy, keyPath).PubKey;
				return new RealbitBlindedAddress(blindingPubKey, base.CreateAddress(derivationStrategy, keyPath, scriptPubKey));
			}

			public static Key GenerateBlindingKey(DerivationStrategyBase derivationStrategy, KeyPath keyPath)
			{
				if (derivationStrategy.Unblinded())
				{
					throw new InvalidOperationException("This derivation scheme is set to only track unblinded addresses");
				}
				var blindingKey = new Key(derivationStrategy.GetChild(keyPath).GetChild(new KeyPath("0")).GetDerivation()
					.ScriptPubKey.WitHash.ToBytes());
				return blindingKey;
			}
		}
		private void InitLiquid(ChainName networkType)
		{
			Add(new LiquidNRXplorerNetwork(NRealbit.Altcoins.Liquid.Instance, networkType)
			{
				MinRPCVersion = 150000,
				CoinType = networkType == ChainName.Mainnet ? new KeyPath("1776'") : new KeyPath("1'"),
			});
		}

		public NRXplorerNetwork GetLBRLB()
		{
			return GetFromCryptoCode(NRealbit.Altcoins.Liquid.Instance.CryptoCode);
		}
	}
	
	public static class LiquidDerivationStrategyOptionsExtensions
	{
		public static bool Unblinded(this DerivationStrategyBase derivationStrategyBase)
		{
			return derivationStrategyBase.AdditionalOptions.TryGetValue("unblinded", out var unblinded) is true && unblinded;
		}
	}
}
