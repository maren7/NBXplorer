using NRealbit;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NRXplorer.Tests
{
	public class RepositoryTester : IDisposable
	{
		public static RepositoryTester Create(bool caching, [CallerMemberName]string name = null)
		{
			return new RepositoryTester(name, caching);
		}

		string _Name;
		private RepositoryProvider _Provider;

		RepositoryTester(string name, bool caching)
		{
			_Name = name;
			ServerTester.DeleteFolderRecursive(name);
			_Provider = new RepositoryProvider(new NRXplorerNetworkProvider(ChainName.Regtest),
											   KeyPathTemplates.Default,
											   new Configuration.ExplorerConfiguration()
											   {
												   DataDir = name,
												   ChainConfigurations = new List<Configuration.ChainConfiguration>()
												   {
													   new Configuration.ChainConfiguration()
													   {
														   CryptoCode = "BRLB",
														   Rescan = false
													   }
												   }
											   });
			_Provider.StartAsync(default).GetAwaiter().GetResult();
			_Repository = _Provider.GetRepository(new NRXplorerNetworkProvider(ChainName.Regtest).GetFromCryptoCode("BRLB"));
		}

		public void Dispose()
		{
			_Provider.StopAsync(default).GetAwaiter().GetResult();
			ServerTester.DeleteFolderRecursive(_Name);
		}

		private Repository _Repository;
		public Repository Repository
		{
			get
			{
				return _Repository;
			}
		}
	}
}
