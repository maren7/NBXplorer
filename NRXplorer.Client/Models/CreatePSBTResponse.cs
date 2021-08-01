using NRealbit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer.Models
{
	public class CreatePSBTResponse
	{
		[JsonProperty("psbt")]
		public PSBT PSBT { get; set; }
		public RealbitAddress ChangeAddress { get; set; }
		public CreatePSBTSuggestions Suggestions { get; set; }

	}
	public class CreatePSBTSuggestions
	{
		public bool ShouldEnforceLowR { get; set; }
	}
}
