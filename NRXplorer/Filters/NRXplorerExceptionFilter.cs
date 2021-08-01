using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using NRXplorer.Models;

namespace NRXplorer.Filters
{
	public class NRXplorerExceptionFilter : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			NRXplorerException ex = null;
			var formatEx = context.Exception as FormatException;
			if(formatEx != null)
			{
				ex = new NRXplorerError(400, "invalid-format", formatEx.Message).AsException();
			}
			ex = ex ?? context.Exception as NRXplorerException;
			if(ex != null)
			{
				context.Exception = null;
				context.ExceptionDispatchInfo = null;
				context.ExceptionHandled = true;
				context.Result = new ObjectResult(ex.Error) { StatusCode = ex.Error.HttpCode };
			}
		}
	}
}
