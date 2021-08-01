using NRXplorer.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer.Models
{
	public class UnknownEvent : NewEventBase
	{
		public UnknownEvent()
		{

		}
		public UnknownEvent(string eventType)
		{
			_EventType = eventType;
		}
		string _EventType;
		public override string EventType => _EventType;

		public JObject Data { get; set; }
	}
}
