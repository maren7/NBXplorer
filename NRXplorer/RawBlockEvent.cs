using NRealbit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRXplorer
{
	public class RawBlockEvent
	{
		public RawBlockEvent(Block block, NRXplorerNetwork network)
		{
			Block = block;
			Network = network;
		}
		public Block Block { get; set; }
		public NRXplorerNetwork Network { get; set; }
	}
}
