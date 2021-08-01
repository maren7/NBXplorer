﻿using NRealbit.RPC;
using Microsoft.Extensions.Logging;
using NRXplorer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NRXplorer.Configuration;
using NRealbit.Protocol;
using System.Threading;
using System.IO;
using NRealbit;
using System.Net;
using NRealbit.Protocol.Behaviors;
using Microsoft.Extensions.Hosting;
using NRXplorer.Events;
using Newtonsoft.Json.Linq;
using System.Text;

namespace NRXplorer
{
	public enum RealbitDWaiterState
	{
		NotStarted,
		CoreSynching,
		NRXplorerSynching,
		Ready
	}

	public class RealbitDWaiters : IHostedService
	{
		Dictionary<string, RealbitDWaiter> _Waiters;
		private readonly AddressPoolService addressPool;
		private readonly NRXplorerNetworkProvider networkProvider;
		private readonly ChainProvider chains;
		private readonly RepositoryProvider repositoryProvider;
		private readonly ExplorerConfiguration config;
		private readonly RPCClientProvider rpcProvider;
		private readonly EventAggregator eventAggregator;

		public RealbitDWaiters(
							AddressPoolService addressPool,
							  NRXplorerNetworkProvider networkProvider,
							  ChainProvider chains,
							  RepositoryProvider repositoryProvider,
							  ExplorerConfiguration config,
							  RPCClientProvider rpcProvider,
							  EventAggregator eventAggregator)
		{
			this.addressPool = addressPool;
			this.networkProvider = networkProvider;
			this.chains = chains;
			this.repositoryProvider = repositoryProvider;
			this.config = config;
			this.rpcProvider = rpcProvider;
			this.eventAggregator = eventAggregator;
		}
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await repositoryProvider.StartCompletion;
			_Waiters = networkProvider
				.GetAll()
				.Select(s => (Repository: repositoryProvider.GetRepository(s),
							  RPCClient: rpcProvider.GetRPCClient(s),
							  Chain: chains.GetChain(s),
							  Network: s))
				.Where(s => s.Repository != null && s.RPCClient != null && s.Chain != null)
				.Select(s => new RealbitDWaiter(s.RPCClient,
												config,
												networkProvider.GetFromCryptoCode(s.Network.CryptoCode),
												s.Chain,
												s.Repository,
												addressPool,
												eventAggregator))
				.ToDictionary(s => s.Network.CryptoCode, s => s);
			await Task.WhenAll(_Waiters.Select(s => s.Value.StartAsync(cancellationToken)).ToArray());
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			await Task.WhenAll(_Waiters.Select(s => s.Value.StopAsync(cancellationToken)).ToArray());
		}

		public RealbitDWaiter GetWaiter(NRXplorerNetwork network)
		{
			return GetWaiter(network.CryptoCode);
		}
		public RealbitDWaiter GetWaiter(string cryptoCode)
		{
			_Waiters.TryGetValue(cryptoCode.ToUpperInvariant(), out RealbitDWaiter waiter);
			return waiter;
		}

		public IEnumerable<RealbitDWaiter> All()
		{
			return _Waiters.Values;
		}
	}

	public class RealbitDWaiter : IHostedService
	{
		RPCClient _OriginalRPC;
		NRXplorerNetwork _Network;
		ExplorerConfiguration _Configuration;
		private ExplorerBehavior _ExplorerPrototype;
		SlimChain _Chain;
		EventAggregator _EventAggregator;
		private readonly ChainConfiguration _ChainConfiguration;
		readonly string RPCReadyFile;

		public RealbitDWaiter(
			RPCClient rpc,
			ExplorerConfiguration configuration,
			NRXplorerNetwork network,
			SlimChain chain,
			Repository repository,
			AddressPoolService addressPoolService,
			EventAggregator eventAggregator)
		{
			if (addressPoolService == null)
				throw new ArgumentNullException(nameof(addressPoolService));
			_OriginalRPC = rpc;
			_Configuration = configuration;
			_Network = network;
			_Chain = chain;
			State = RealbitDWaiterState.NotStarted;
			_EventAggregator = eventAggregator;
			_ChainConfiguration = _Configuration.ChainConfigurations.First(c => c.CryptoCode == _Network.CryptoCode);
			_ExplorerPrototype = new ExplorerBehavior(repository, chain, addressPoolService, eventAggregator) { StartHeight = _ChainConfiguration.StartHeight };
			RPCReadyFile = Path.Combine(configuration.SignalFilesDir, $"{network.CryptoCode.ToLowerInvariant()}_fully_synched");
			HasTxIndex = _ChainConfiguration.HasTxIndex;
		}
		public NodeState NodeState
		{
			get;
			private set;
		}

		private Node _Node;


		public NRXplorerNetwork Network
		{
			get
			{
				return _Network;
			}
		}

		public RPCClient RPC
		{
			get
			{
				return _OriginalRPC;
			}
		}

		public RealbitDWaiterState State
		{
			get;
			private set;
		}



		public bool RPCAvailable
		{
			get
			{
				return State == RealbitDWaiterState.Ready ||
					State == RealbitDWaiterState.CoreSynching ||
					State == RealbitDWaiterState.NRXplorerSynching;
			}
		}
		IDisposable _Subscription;
		Task _Loop;
		CancellationTokenSource _Cts;
		public Task StartAsync(CancellationToken cancellationToken)
		{
			if (_Disposed)
				throw new ObjectDisposedException(nameof(RealbitDWaiter));

			_Cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_Loop = StartLoop(_Cts.Token, _Tick);
			_Subscription = _EventAggregator.Subscribe<Models.NewBlockEvent>(s =>
			{
				_Tick.Set();
			});
			return Task.CompletedTask;
		}

		Signaler _Tick = new Signaler();

		private async Task StartLoop(CancellationToken token, Signaler tick)
		{
			try
			{
				int errors = 0;
				while (!token.IsCancellationRequested)
				{
					errors = Math.Min(11, errors);
					try
					{
						while (await StepAsync(token))
						{
						}
						await tick.Wait(PollingInterval, token);
						errors = 0;
					}
					catch (ConfigException) when (!token.IsCancellationRequested)
					{
						// Probably RPC errors, don't spam
						await Wait(errors, tick, token);
						errors++;
					}
					catch (Exception ex) when (!token.IsCancellationRequested)
					{
						Logs.Configuration.LogError(ex, $"{_Network.CryptoCode}: Unhandled in Waiter loop");
						await Wait(errors, tick, token);
						errors++;
					}
				}
			}
			catch when (token.IsCancellationRequested)
			{
			}
			finally
			{
				EnsureNodeDisposed();
			}
		}

		private async Task Wait(int errors, Signaler tick, CancellationToken token)
		{
			var timeToWait = TimeSpan.FromSeconds(5.0) * (errors + 1);
			Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Testing again in {(int)timeToWait.TotalSeconds} seconds");
			await tick.Wait(timeToWait, token);
		}

		public BlockLocator GetLocation()
		{
			return GetExplorerBehavior()?.CurrentLocation;
		}

		public TimeSpan PollingInterval
		{
			get; set;
		} = TimeSpan.FromMinutes(1.0);

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_Disposed = true;
			_Cts.Cancel();
			_Subscription.Dispose();
			EnsureNodeDisposed();
			State = RealbitDWaiterState.NotStarted;
			_Chain = null;
			try
			{
				await _Loop;
			}
			catch { }
			EnsureRPCReadyFileDeleted();
		}
		bool _BanListLoaded;
		async Task<bool> StepAsync(CancellationToken token)
		{
			var oldState = State;
			switch (State)
			{
				case RealbitDWaiterState.NotStarted:
					await RPCArgs.TestRPCAsync(_Network, _OriginalRPC, token);
					_OriginalRPC.Capabilities = _OriginalRPC.Capabilities;
					GetBlockchainInfoResponse blockchainInfo = null;
					try
					{
						blockchainInfo = await _OriginalRPC.GetBlockchainInfoAsyncEx();
						if (_Network.CryptoCode == "BRLB" &&
							_Network.NRealbitNetwork.ChainName == ChainName.Mainnet &&
							!_BanListLoaded)
						{
							try
							{
								await LoadBanList();
								_BanListLoaded = true;
							}
							catch (Exception ex)
							{
								Logs.Explorer.LogWarning($"{Network.CryptoCode}: Error while trying to load the ban list, skipping... ({ex.Message})");
							}
						}
						if (blockchainInfo != null && _Network.NRealbitNetwork.ChainName == ChainName.Regtest)
						{
							if (await WarmupBlockchain())
							{
								blockchainInfo = await _OriginalRPC.GetBlockchainInfoAsyncEx();
							}
						}
					}
					catch (Exception ex)
					{
						Logs.Configuration.LogError(ex, $"{_Network.CryptoCode}: Failed to connect to RPC");
						break;
					}
					if (IsSynchingCore(blockchainInfo))
					{
						State = RealbitDWaiterState.CoreSynching;
					}
					else
					{
						await ConnectToRealbitD(token);
						State = RealbitDWaiterState.NRXplorerSynching;
					}
					break;
				case RealbitDWaiterState.CoreSynching:
					GetBlockchainInfoResponse blockchainInfo2 = null;
					try
					{
						blockchainInfo2 = await _OriginalRPC.GetBlockchainInfoAsyncEx();
					}
					catch (Exception ex)
					{
						Logs.Configuration.LogError(ex, $"{_Network.CryptoCode}: Failed to connect to RPC");
						State = RealbitDWaiterState.NotStarted;
						break;
					}
					if (!IsSynchingCore(blockchainInfo2))
					{
						await ConnectToRealbitD(token);
						State = RealbitDWaiterState.NRXplorerSynching;
					}
					break;
				case RealbitDWaiterState.NRXplorerSynching:
					var explorer = GetExplorerBehavior();
					if (explorer == null)
					{
						State = RealbitDWaiterState.NotStarted;
					}
					else if (!explorer.IsSynching())
					{
						State = RealbitDWaiterState.Ready;
					}
					break;
				case RealbitDWaiterState.Ready:
					var explorer2 = GetExplorerBehavior();
					if (explorer2 == null)
					{
						State = RealbitDWaiterState.NotStarted;
					}
					else if (explorer2.IsSynching())
					{
						State = RealbitDWaiterState.NRXplorerSynching;
					}
					break;
				default:
					break;
			}
			var changed = oldState != State;

			if (changed)
			{
				if (oldState == RealbitDWaiterState.NotStarted)
					NetworkInfo = await _OriginalRPC.GetNetworkInfoAsync();
				_EventAggregator.Publish(new RealbitDStateChangedEvent(_Network, oldState, State));
				if (State == RealbitDWaiterState.Ready)
				{
					await File.WriteAllTextAsync(RPCReadyFile, NRealbit.Utils.DateTimeToUnixTime(DateTimeOffset.UtcNow).ToString());
				}
			}
			if (State != RealbitDWaiterState.Ready)
			{
				EnsureRPCReadyFileDeleted();
			}
			return changed;
		}

		private Node GetHandshakedNode()
		{
			return _Node?.State == NodeState.HandShaked ? _Node : null;
		}

		internal ExplorerBehavior GetExplorerBehavior()
		{
			return GetHandshakedNode()?.Behaviors?.Find<ExplorerBehavior>();
		}

		private void EnsureRPCReadyFileDeleted()
		{
			if (File.Exists(RPCReadyFile))
				File.Delete(RPCReadyFile);
		}

		private async Task LoadBanList()
		{
			var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("NRXplorer.banlist.cli.txt");
			string content = null;
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				content = reader.ReadToEnd();
			}
			var bannedLines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var batch = _OriginalRPC.PrepareBatch();
			var commands = bannedLines
						.Where(o => o.Length > 0 && o[0] != '#')
						.Select(b => b.Split(' ')[2])
						.Select(ip => batch.SendCommandAsync(new RPCRequest("setban", new object[] { ip, "add", 31557600 }) { ThrowIfRPCError = false }))
						.ToArray();
			await batch.SendBatchAsync();
			foreach (var command in commands)
			{
				var result = await command;
				if (result.Error != null && result.Error.Code != RPCErrorCode.RPC_CLIENT_NODE_ALREADY_ADDED)
					result.ThrowIfError();
			}
			Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Node banlist loaded");
		}

		private async Task ConnectToRealbitD(CancellationToken cancellation)
		{
			var node = GetHandshakedNode();
			if (node != null)
				return;
			try
			{
				EnsureNodeDisposed();
				_Chain.ResetToGenesis();
				if (_Configuration.CacheChain)
				{
					LoadChainFromCache();
					if (!await HasBlock(_OriginalRPC, _Chain.Tip))
					{
						Logs.Configuration.LogInformation($"{_Network.CryptoCode}: The cached chain contains a tip unknown to the node, dropping the cache...");
						_Chain.ResetToGenesis();
					}
				}
				var heightBefore = _Chain.Height;
				using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
				{
					timeout.CancelAfter(_Network.ChainLoadingTimeout);
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Trying to connect via the P2P protocol to trusted node ({_ChainConfiguration.NodeEndpoint.ToEndpointString()})...");
					var userAgent = "NRXplorer-" + RandomUtils.GetInt64();
					bool handshaked = false;
					bool connected = false;
					bool chainLoaded = false;
					using (var handshakeTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
					{
						try
						{
							handshakeTimeout.CancelAfter(TimeSpan.FromSeconds(10));
							node = await Node.ConnectAsync(_Network.NRealbitNetwork, _ChainConfiguration.NodeEndpoint, new NodeConnectionParameters()
							{
								UserAgent = userAgent,
								ConnectCancellation = handshakeTimeout.Token,
								IsRelay = true
							});
							connected = true;
							Logs.Explorer.LogInformation($"{Network.CryptoCode}: TCP Connection succeed, handshaking...");
							node.VersionHandshake(handshakeTimeout.Token);
							handshaked = true;
							Logs.Explorer.LogInformation($"{Network.CryptoCode}: Handshaked");
							var loadChainTimeout = _Network.NRealbitNetwork.ChainName == ChainName.Regtest ? TimeSpan.FromSeconds(5) : _Network.ChainCacheLoadingTimeout;
							if (_Chain.Height < 5)
								loadChainTimeout = TimeSpan.FromDays(7); // unlimited
							Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Loading chain from node");
							try
							{
								using (var cts1 = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
								{
									cts1.CancelAfter(loadChainTimeout);
									Logs.Explorer.LogInformation($"{Network.CryptoCode}: Loading chain...");
									node.SynchronizeSlimChain(_Chain, cancellationToken: cts1.Token);
								}
							}
							catch when (!cancellation.IsCancellationRequested) // Timeout happens with SynchronizeChain, if so, throw away the cached chain
							{
								Logs.Explorer.LogInformation($"{Network.CryptoCode}: Failed to load chain before timeout, let's try again without the chain cache...");
								_Chain.ResetToGenesis();
								node.SynchronizeSlimChain(_Chain, cancellationToken: cancellation);
							}
							Logs.Explorer.LogInformation($"{Network.CryptoCode}: Chain loaded");
							chainLoaded = true;
							var peer = (await _OriginalRPC.GetPeersInfoAsync())
										.FirstOrDefault(p => p.SubVersion == userAgent);
							if (IsWhitelisted(peer))
							{
								Logs.Explorer.LogInformation($"{Network.CryptoCode}: NRXplorer is correctly whitelisted by the node");
							}
							else
							{
								var addressStr = peer.Address is IPEndPoint end ? end.Address.ToString() : peer.Address?.ToString();
								Logs.Explorer.LogWarning($"{Network.CryptoCode}: Your NRXplorer server is not whitelisted by your node," +
									$" you should add \"whitelist={addressStr}\" to the configuration file of your node. (Or use whitebind)");
							}
						}
						catch
						{
							if (!connected)
							{
								Logs.Explorer.LogWarning($"{Network.CryptoCode}: NRXplorer failed to connect to the node via P2P ({_ChainConfiguration.NodeEndpoint.ToEndpointString()}).{Environment.NewLine}" +
									$"It may come from: A firewall blocking the traffic, incorrect IP or port, or your node may not have an available connection slot. {Environment.NewLine}" +
									$"To make sure your node have an available connection slot, use \"whitebind\" or \"whitelist\" in your node configuration. (typically whitelist=127.0.0.1 if NRXplorer and the node are on the same machine.){Environment.NewLine}");
							}
							else if (!handshaked)
							{
								Logs.Explorer.LogWarning($"{Network.CryptoCode}: NRXplorer connected to the remote node but failed to handhsake via P2P.{Environment.NewLine}" +
									$"Your node may not have an available connection slot, or you may try to connect to the wrong node. (ie, trying to connect to a LTC node on the BRLB configuration).{Environment.NewLine}" +
									$"To make sure your node have an available connection slot, use \"whitebind\" or \"whitelist\" in your node configuration. (typically whitelist=127.0.0.1 if NRXplorer and the node are on the same machine.){Environment.NewLine}");
							}
							else if (!chainLoaded)
							{
								Logs.Explorer.LogWarning($"{Network.CryptoCode}: NRXplorer connected and handshaked the remote node but failed to load the chain of header.{Environment.NewLine}" +
									$"Your connection may be throttled, or you may try to connect to the wrong node. (ie, trying to connect to a LTC node on the BRLB configuration).{Environment.NewLine}");
							}
							throw;
						}
					}
				}
				Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Height: " + _Chain.Height);
				if (_Configuration.CacheChain && heightBefore != _Chain.Height)
				{
					SaveChainInCache();
				}
				GC.Collect();
				node.Behaviors.Add(new SlimChainBehavior(_Chain));
				var explorer = (ExplorerBehavior)_ExplorerPrototype.Clone();
				node.Behaviors.Add(explorer);
				node.StateChanged += Node_StateChanged;
				_Node = node;
				await explorer.Init();
			}
			catch
			{
				EnsureNodeDisposed(node ?? _Node);
				throw;
			}
		}

		private bool IsWhitelisted(PeerInfo peer)
		{
			if (peer is null)
				return false;
			if (peer.IsWhiteListed)
				return true;
			if (peer.Permissions.Contains("noban", StringComparer.OrdinalIgnoreCase))
				return true;
			return false;
		}

		private void Node_StateChanged(Node node, NodeState oldState)
		{
			_Tick.Set();
		}

		private void EnsureNodeDisposed(Node node = null)
		{
			node = node ?? _Node;
			if (node != null)
			{
				try
				{
					node.StateChanged -= Node_StateChanged;
					node.DisconnectAsync();
				}
				catch { }
				node = null;
				_Node = null;
			}
		}

		private async Task<bool> HasBlock(RPCClient rpc, uint256 tip)
		{
			try
			{
				await rpc.GetBlockHeaderAsync(tip);
				return true;
			}
			catch (RPCException r) when (r.RPCCode == RPCErrorCode.RPC_METHOD_NOT_FOUND)
			{
				try
				{
					await rpc.GetBlockAsync(tip);
					return true;
				}
				catch
				{
					return false;
				}
			}
			catch (RPCException r) when (r.RPCCode == RPCErrorCode.RPC_INVALID_ADDRESS_OR_KEY || r.RPCCode == RPCErrorCode.RPC_INVALID_PARAMETER)
			{
				return false;
			}
		}

		private void SaveChainInCache()
		{
			var suffix = _Network.CryptoCode == "BRLB" ? "" : _Network.CryptoCode;
			var cachePath = Path.Combine(_Configuration.DataDir, $"{suffix}chain-slim.dat");
			var cachePathTemp = Path.Combine(_Configuration.DataDir, $"{suffix}chain-slim.dat.temp");

			Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Saving chain to cache...");
			using (var fs = new FileStream(cachePathTemp, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024))
			{
				_Chain.Save(fs);
				fs.Flush();
			}

			if (File.Exists(cachePath))
				File.Delete(cachePath);
			File.Move(cachePathTemp, cachePath);
			Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Chain cached");
		}

		private void LoadChainFromCache()
		{
			var suffix = _Network.CryptoCode == "BRLB" ? "" : _Network.CryptoCode;
			{
				var legacyCachePath = Path.Combine(_Configuration.DataDir, $"{suffix}chain.dat");
				if (_Configuration.CacheChain && File.Exists(legacyCachePath))
				{
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Loading chain from cache...");
					var chain = new ConcurrentChain(_Network.NRealbitNetwork);
					chain.Load(File.ReadAllBytes(legacyCachePath), _Network.NRealbitNetwork);
					LoadSlimAndSaveToSlimFormat(chain);
					File.Delete(legacyCachePath);
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Height: " + _Chain.Height);
					return;
				}
			}

			{
				var cachePath = Path.Combine(_Configuration.DataDir, $"{suffix}chain-stripped.dat");
				if (_Configuration.CacheChain && File.Exists(cachePath))
				{
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Loading chain from cache...");
					var chain = new ConcurrentChain(_Network.NRealbitNetwork);
					chain.Load(File.ReadAllBytes(cachePath), _Network.NRealbitNetwork, new ConcurrentChain.ChainSerializationFormat()
					{
						SerializeBlockHeader = false,
						SerializePrecomputedBlockHash = true,
					});
					LoadSlimAndSaveToSlimFormat(chain);
					File.Delete(cachePath);
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Height: " + _Chain.Height);
					return;
				}
			}

			{
				var slimCachePath = Path.Combine(_Configuration.DataDir, $"{suffix}chain-slim.dat");
				if (_Configuration.CacheChain && File.Exists(slimCachePath))
				{
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Loading chain from cache...");
					using (var file = new FileStream(slimCachePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024))
					{
						_Chain.Load(file);
					}
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Height: " + _Chain.Height);
					return;
				}
			}
		}

		private void LoadSlimAndSaveToSlimFormat(ConcurrentChain chain)
		{
			foreach (var block in chain.ToEnumerable(false))
			{
				_Chain.TrySetTip(block.HashBlock, block.Previous?.HashBlock);
			}
			SaveChainInCache();
		}

		private async Task<bool> WarmupBlockchain()
		{
			if (await _OriginalRPC.GetBlockCountAsync() < _Network.NRealbitNetwork.Consensus.CoinbaseMaturity)
			{
				Logs.Configuration.LogInformation($"{_Network.CryptoCode}: Less than {_Network.NRealbitNetwork.Consensus.CoinbaseMaturity} blocks, mining some block for regtest");
				await _OriginalRPC.EnsureGenerateAsync(_Network.NRealbitNetwork.Consensus.CoinbaseMaturity + 1);
				return true;
			}
			else
			{
				var hash = await _OriginalRPC.GetBestBlockHashAsync();

				BlockHeader header = null;
				try
				{
					header = await _OriginalRPC.GetBlockHeaderAsync(hash);
				}
				catch (RPCException ex) when (ex.RPCCode == RPCErrorCode.RPC_METHOD_NOT_FOUND)
				{
					header = (await _OriginalRPC.GetBlockAsync(hash)).Header;
				}
				if ((DateTimeOffset.UtcNow - header.BlockTime) > TimeSpan.FromSeconds(24 * 60 * 60))
				{
					Logs.Configuration.LogInformation($"{_Network.CryptoCode}: It has been a while nothing got mined on regtest... mining 10 blocks");
					await _OriginalRPC.GenerateAsync(10);
					return true;
				}
				return false;
			}
		}

		public bool IsSynchingCore(GetBlockchainInfoResponse blockchainInfo)
		{
			if (blockchainInfo.InitialBlockDownload == true)
				return true;
			if (blockchainInfo.MedianTime.HasValue && _Network.NRealbitNetwork.ChainName != ChainName.Regtest)
			{
				var time = NRealbit.Utils.UnixTimeToDateTime(blockchainInfo.MedianTime.Value);
				// 5 month diff? probably synching...
				if (DateTimeOffset.UtcNow - time > TimeSpan.FromDays(30 * 5))
				{
					return true;
				}
			}

			return blockchainInfo.Headers - blockchainInfo.Blocks > 6;
		}

		bool _Disposed = false;

		public bool Connected
		{
			get
			{
				return GetHandshakedNode() != null;
			}
		}

		public GetNetworkInfoResponse NetworkInfo { get; internal set; }
		public bool HasTxIndex { get; set; }
	}
}
