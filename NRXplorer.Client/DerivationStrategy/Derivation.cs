using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer.DerivationStrategy
{
	public class Derivation
	{
		public Derivation()
		{

		}
		public Script ScriptPubKey
		{
			get; set;
		}
		public Script Redeem
		{
			get; set;
		}
	}
}
