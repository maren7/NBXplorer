using NRealbit;
using Newtonsoft.Json;

namespace NRXplorer.Models
{
	public class NewBlockEvent : NewEventBase
	{
		public NewBlockEvent()
		{
		}
		public int Height
		{
			get; set;
		}

		public uint256 Hash
		{
			get; set;
		}
		public uint256 PreviousBlockHash
		{
			get; set;
		}

		[JsonIgnore]
		public override string EventType => "newblock";

		public override string ToString()
		{
			return $"{CryptoCode}: New block {Hash} ({Height})";
		}
	}
}
