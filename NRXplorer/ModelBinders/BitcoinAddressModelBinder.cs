using Microsoft.AspNetCore.Mvc.ModelBinding;
using NRealbit;
using System.Reflection;
using System;
using System.Threading.Tasks;
using NRXplorer.DerivationStrategy;

namespace NRXplorer.ModelBinders
{
	public class RealbitAddressModelBinder : IModelBinder
	{
		public RealbitAddressModelBinder()
		{

		}

		#region IModelBinder Members

		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (!typeof(RealbitAddress).GetTypeInfo().IsAssignableFrom(bindingContext.ModelType))
			{
				return Task.CompletedTask;
			}

			ValueProviderResult val = bindingContext.ValueProvider.GetValue(
				bindingContext.ModelName);
			if (val == null)
			{
				return Task.CompletedTask;
			}

			string key = val.FirstValue as string;
			if (key == null)
			{
				return Task.CompletedTask;
			}

			var networkProvider = (NRXplorer.NRXplorerNetworkProvider)bindingContext.HttpContext.RequestServices.GetService(typeof(NRXplorer.NRXplorerNetworkProvider));
			var cryptoCode = bindingContext.ValueProvider.GetValue("cryptoCode").FirstValue;
			var network = networkProvider.GetFromCryptoCode(cryptoCode ?? "BRLB");
			try
			{
				var data = RealbitAddress.Create(key, network.NRealbitNetwork);
				if (!bindingContext.ModelType.IsInstanceOfType(data))
				{
					throw new FormatException("Invalid address");
				}
				bindingContext.Result = ModelBindingResult.Success(data);
			}
			catch { throw new FormatException("Invalid address"); }
			return Task.CompletedTask;
		}

		#endregion
	}
}
