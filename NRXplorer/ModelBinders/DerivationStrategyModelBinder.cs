﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using NRealbit;
using System.Reflection;
using System;
using System.Threading.Tasks;
using NRXplorer.DerivationStrategy;

namespace NRXplorer.ModelBinders
{
	public class DerivationStrategyModelBinder : IModelBinder
	{
		public DerivationStrategyModelBinder()
		{

		}

		#region IModelBinder Members

		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if(!typeof(DerivationStrategyBase).GetTypeInfo().IsAssignableFrom(bindingContext.ModelType))
			{
				return Task.CompletedTask;
			}

			ValueProviderResult val = bindingContext.ValueProvider.GetValue(
				bindingContext.ModelName);
			if(val == null)
			{
				return Task.CompletedTask;
			}

			string key = val.FirstValue as string;
			if(key == null)
			{
				return Task.CompletedTask;
			}

			var networkProvider = (NRXplorer.NRXplorerNetworkProvider)bindingContext.HttpContext.RequestServices.GetService(typeof(NRXplorer.NRXplorerNetworkProvider));
			var cryptoCode = bindingContext.ValueProvider.GetValue("cryptoCode").FirstValue;
			cryptoCode = cryptoCode ?? bindingContext.ValueProvider.GetValue("network").FirstValue;
			var network = networkProvider.GetFromCryptoCode((cryptoCode ?? "BRLB"));
			try
			{
				var data = network.DerivationStrategyFactory.Parse(key);
				if(!bindingContext.ModelType.IsInstanceOfType(data))
				{
					throw new FormatException("Invalid destination type");
				}
				bindingContext.Result = ModelBindingResult.Success(data);
			}
			catch { throw new FormatException("Invalid derivation scheme"); }
			return Task.CompletedTask;
		}

		#endregion
	}
}
