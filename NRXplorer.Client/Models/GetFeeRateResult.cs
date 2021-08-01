using NRealbit;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer.Models
{
    public class GetFeeRateResult
    {
		public FeeRate FeeRate
		{
			get; set;
		}

		public int BlockCount
		{
			get; set;
		}
	}
}
