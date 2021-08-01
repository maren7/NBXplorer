using NRealbit.Tests;
using System;
using System.Collections.Generic;
using System.Text;
using NRealbit;

namespace NRXplorer.Tests
{
    public partial class ServerTester
    {
		NRXplorerNetworkProvider _Provider = new NRXplorerNetworkProvider(ChainName.Regtest);
		private void SetEnvironment()
		{
			//CryptoCode = "AGM";
			//nodeDownloadData = NodeDownloadData.Argoneum.v1_4_1;
			//Network = NRealbit.Altcoins.Argoneum.Instance.Regtest;

			//CryptoCode = "LTC";
			//nodeDownloadData = NodeDownloadData.Litecoin.v0_16_3;
			//Network = NRealbit.Altcoins.Litecoin.Instance.Regtest;

			//CryptoCode = "BCH";
			//nodeDownloadData = NodeDownloadData.BCash.v22_1_0;
			//NRXplorerNetwork = _Provider.GetBCH();

			//Tests of DOGE are broken because it outpoint locking seems to work differently
			//CryptoCode = "DOGE";
			//nodeDownloadData = NodeDownloadData.Dogecoin.v1_10_0;
			//Network = NRealbit.Altcoins.Dogecoin.Instance.Regtest;
			//RPCStringAmount = false;

			//CryptoCode = "DASH";
			//nodeDownloadData = NodeDownloadData.Dash.v0_12_2;
			//Network = NRealbit.Altcoins.Dash.Instance.Regtest;

			//CryptoCode = "TRC";
			//nodeDownloadData = NodeDownloadData.Terracoin.v0_12_2;
			//Network = NRealbit.Altcoins.Terracoin.Instance.Regtest;

			//CryptoCode = "POLIS";
			//nodeDownloadData = NodeDownloadData.Polis.v1_3_1;
			//Network = NRealbit.Altcoins.Polis.Instance.Regtest;

			//CryptoCode = "BTG";
			//nodeDownloadData = NodeDownloadData.BGold.v0_15_0;
			//Network = NRealbit.Altcoins.BGold.Instance.Regtest;

			//CryptoCode = "MONA";
			//nodeDownloadData = NodeDownloadData.Monacoin.v0_15_1;
			//Network = NRealbit.Altcoins.Monacoin.Instance.Regtest;

			//CryptoCode = "FTC";
			//nodeDownloadData = NodeDownloadData.Feathercoin.v0_16_0;
			//Network = NRealbit.Altcoins.Feathercoin.Instance.Regtest;

			//CryptoCode = "UFO";
			//nodeDownloadData = NodeDownloadData.Ufo.v0_16_0;
			//Network = NRealbit.Altcoins.Ufo.Instance.Regtest;

			//CryptoCode = "VIA";
			//nodeDownloadData = NodeDownloadData.Viacoin.v0_15_1;
			//Network = NRealbit.Altcoins.Viacoin.Instance.Regtest;

			//CryptoCode = "GRS";
			//nodeDownloadData = NodeDownloadData.Groestlcoin.v2_16_0;
			//Network = NRealbit.Altcoins.Groestlcoin.Instance.Regtest;

			//CryptoCode = "BTX";
			//nodeDownloadData = NodeDownloadData.Bitcore.v0_90_9_10;
			//Network = NRealbit.Altcoins.Bitcore.Instance.Regtest;

			//CryptoCode = "XMCC";
			//nodeDownloadData = NodeDownloadData.Monoeci.v0_12_2_3;
			//Network = NRealbit.Altcoins.Monoeci.Instance.Regtest;
			//RPCSupportSegwit = false;

			//CryptoCode = "GBX";
			//nodeDownloadData = NodeDownloadData.Gobyte.v0_12_2_4;
			//Network = NRealbit.Altcoins.Gobyte.Instance.Regtest;
			//RPCSupportSegwit = false;

			//CryptoCode = "COLX";
			//nodeDownloadData = NodeDownloadData.Colossus.v1_1_1;
			//Network = NRealbit.Altcoins.Colossus.Instance.Regtest;
			//RPCSupportSegwit = false;

			//CryptoCode = "QTUM";
			//nodeDownloadData = NodeDownloadData.Qtum.v0_18_3;
			//NRXplorerNetwork = _Provider.GetQTUM();

			//CryptoCode = "MUE";
			//nodeDownloadData = NodeDownloadData.MonetaryUnit.v2_1_6;
			//Network = NRealbit.Altcoins.MonetaryUnit.Instance.Regtest;

			//CryptoCode = "LBRLB";
			//nodeDownloadData = NodeDownloadData.Elements.v0_18_1_1;
			//NRXplorerNetwork = _Provider.GetLBRLB();
			//
			CryptoCode = "BRLB";
			nodeDownloadData = NodeDownloadData.Realbit.v0_21_0;
			NRXplorerNetwork = _Provider.GetBRLB();
		}
	}
}
