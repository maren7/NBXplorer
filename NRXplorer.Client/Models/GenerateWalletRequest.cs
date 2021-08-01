using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using NRXplorer.JsonConverters;
using NRealbit;

namespace NRXplorer.Models
{
	public class GenerateWalletRequest
	{
		public int AccountNumber { get; set; }		
		public string ExistingMnemonic { get; set; }
		[JsonConverter(typeof(NRXplorer.JsonConverters.WordlistJsonConverter))]
		public NRealbit.Wordlist WordList { get; set; }
		[JsonConverter(typeof(NRXplorer.JsonConverters.WordcountJsonConverter))]
		public NRealbit.WordCount? WordCount { get; set; }
		[JsonConverter(typeof(NRXplorer.JsonConverters.ScriptPubKeyTypeConverter))]
		public NRealbit.ScriptPubKeyType? ScriptPubKeyType { get; set; }
		public string Passphrase { get; set; }
		public bool ImportKeysToRPC { get; set; }
		public bool SavePrivateKeys { get; set; }
	}
}
