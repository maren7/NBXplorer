using System;
using System.Collections.Generic;
using System.Text;

namespace NRXplorer.Models
{
	public class NRXplorerException : Exception
	{
		public NRXplorerException(NRXplorerError error):base(error.Message)
		{
			Error = error;
		}

		public NRXplorerError Error
		{
			get; set;
		}
	}
	public class NRXplorerError
	{
		public NRXplorerError()
		{

		}
		public NRXplorerError(int httpCode, string code, string message)
		{
			HttpCode = httpCode;
			Code = code;
			Message = message;
		}
		public int HttpCode
		{
			get; set;
		}
		public string Code
		{
			get; set;
		}
		public string Message
		{
			get; set;
		}

		public NRXplorerException AsException()
		{
			return new NRXplorerException(this);
		}
	}
}
