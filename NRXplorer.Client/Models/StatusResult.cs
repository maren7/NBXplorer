using NRealbit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer.Models
{
	public class RealbitStatus
	{
		public int Blocks
		{
			get; set;
		}
		public int Headers
		{
			get; set;
		}
		public double VerificationProgress
		{
			get; set;
		}
		public bool IsSynched
		{
			get;
			set;
		}
		public FeeRate IncrementalRelayFee
		{
			get;
			set;
		}
		public FeeRate MinRelayTxFee
		{
			get;
			set;
		}
		public string[] ExternalAddresses { get; set; }
		public NodeCapabilities Capabilities { get; set; }
	}
	public class NodeCapabilities
	{
		public bool CanScanTxoutSet { get; set; }
		public bool CanSupportSegwit { get; set; }
		public bool CanSupportTransactionCheck { get; set; }
	}
	public class StatusResult
    {
		public RealbitStatus RealbitStatus
		{
			get; set;
		}
		public double RepositoryPingTime
		{
			get;
			set;
		}
		public bool IsFullySynched
		{
			get; set;
		}
		public int ChainHeight
		{
			get;
			set;
		}
		public int? SyncHeight
		{
			get;
			set;
		}
		public string InstanceName
		{
			get;
			set;
		}
		[JsonConverter(typeof(NRealbit.JsonConverters.ChainNameJsonConverter))]
		public ChainName NetworkType
		{
			get;
			set;
		}
		public string CryptoCode
		{
			get;
			set;
		}

		public string[] SupportedCryptoCodes
		{
			get; set;
		}
		public string Version
		{
			get;
			set;
		}
	}
}
