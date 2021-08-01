using NRealbit;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer
{
	public class Serializer
	{

		private readonly NRXplorerNetwork _Network;
		public Network Network => _Network?.NRealbitNetwork;

		public JsonSerializerSettings Settings { get; } = new JsonSerializerSettings();
		public Serializer(NRXplorerNetwork network)
		{
			_Network = network;
			ConfigureSerializer(Settings);
		}

		public void ConfigureSerializer(JsonSerializerSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));
			NRealbit.JsonConverters.Serializer.RegisterFrontConverters(settings, Network);
			if (_Network != null)
			{
				settings.Converters.Insert(0, new JsonConverters.CachedSerializer(_Network));
			}
			ReplaceConverter<NRealbit.JsonConverters.MoneyJsonConverter>(settings, new NRXplorer.JsonConverters.MoneyJsonConverter());
		}

		private static void ReplaceConverter<T>(JsonSerializerSettings settings, JsonConverter jsonConverter) where T : JsonConverter
		{
			var moneyConverter = settings.Converters.OfType<T>().Single();
			var index = settings.Converters.IndexOf(moneyConverter);
			settings.Converters.RemoveAt(index);
			settings.Converters.Insert(index, new NRXplorer.JsonConverters.MoneyJsonConverter());
		}

		public T ToObject<T>(string str)
		{
			return JsonConvert.DeserializeObject<T>(str, Settings);
		}

		public string ToString<T>(T obj)
		{
			return JsonConvert.SerializeObject(obj, Settings);
		}

		public T ToObject<T>(JObject jobj)
		{
			var serializer = JsonSerializer.Create(Settings);
			return jobj.ToObject<T>(serializer);
		}
	}
}
