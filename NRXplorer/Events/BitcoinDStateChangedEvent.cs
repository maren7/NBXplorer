using NRXplorer.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRXplorer.Events
{
    public class RealbitDStateChangedEvent
	{
		public RealbitDStateChangedEvent(NRXplorerNetwork network, RealbitDWaiterState oldState, RealbitDWaiterState newState)
		{
			OldState = oldState;
			NewState = newState;
			Network = network;
		}

		public NRXplorerNetwork Network
		{
			get; set;
		}

		public RealbitDWaiterState OldState
		{
			get; set;
		}
		public RealbitDWaiterState NewState
		{
			get; set;
		}

		public override string ToString()
		{
			return ($"{Network.CryptoCode}: Node state changed: {OldState} => {NewState}");
		}
	}
}
