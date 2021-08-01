using Microsoft.AspNetCore.Mvc.ModelBinding;
using NRealbit;
using System.Reflection;
using System;
using System.Threading.Tasks;
using NRXplorer.DerivationStrategy;

namespace NRXplorer.ModelBinders
{
	public class NetworkModelBinder : IModelBinder
	{
		public NetworkModelBinder()
		{

		}

		#region IModelBinder Members

		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if(!typeof(Network).GetTypeInfo().IsAssignableFrom(bindingContext.ModelType) &&
			   !typeof(NRXplorerNetwork).GetTypeInfo().IsAssignableFrom(bindingContext.ModelType))
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
			if (string.IsNullOrEmpty(cryptoCode))
			{
				cryptoCode = bindingContext.ValueProvider.GetValue("network").FirstValue;
			}
			if (string.IsNullOrEmpty(cryptoCode))
			{
				cryptoCode = "BRLB";
			}
			var network = networkProvider.GetFromCryptoCode(cryptoCode);
			if (network == null)
				throw new FormatException($"The cryptoCode '{cryptoCode}' is not supported");
			if (typeof(Network).GetTypeInfo().IsAssignableFrom(bindingContext.ModelType))
				bindingContext.Result = ModelBindingResult.Success(network.NRealbitNetwork);
			else
				bindingContext.Result = ModelBindingResult.Success(network);
			return Task.CompletedTask;
		}

		#endregion
	}
}
