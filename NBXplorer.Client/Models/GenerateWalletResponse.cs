using NRealbit;
using NRXplorer.DerivationStrategy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using NRXplorer.JsonConverters;

namespace NRXplorer.Models
{
	public class GenerateWalletResponse
	{
		public string Mnemonic { get; set; }
		public string Passphrase { get; set; }
		[JsonConverter(typeof(NRXplorer.JsonConverters.WordlistJsonConverter))]
		public NRealbit.Wordlist WordList { get; set; }
		[JsonConverter(typeof(NRXplorer.JsonConverters.WordcountJsonConverter))]
		public NRealbit.WordCount WordCount { get; set; }
		public RealbitExtKey MasterHDKey { get; set; }
		public RealbitExtKey AccountHDKey { get; set; }
		[JsonConverter(typeof(NRealbit.JsonConverters.KeyPathJsonConverter))]
		public NRealbit.RootedKeyPath AccountKeyPath { get; set; }
		public DerivationStrategyBase DerivationScheme { get; set; }

		public Mnemonic GetMnemonic()
		{
			return new Mnemonic(Mnemonic, WordList);
		}
	}
}
