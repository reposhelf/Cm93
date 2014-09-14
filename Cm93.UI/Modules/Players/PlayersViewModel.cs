﻿/*
        Copyright © Iain McDonald 2013-2014
        This file is part of Cm93.

        Cm93 is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        Cm93 is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with Cm93. If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Caliburn.Micro;
using Cm93.Model.Attributes;
using Cm93.Model.Enumerations;
using Cm93.Model.Interfaces;
using Cm93.Model.Modules;
using Cm93.Model.Structures;
using Cm93.UI.Events;

namespace Cm93.UI.Modules.Players
{
	[Export(typeof(ModuleViewModelBase))]
	public class PlayersViewModel : ModuleViewModelBase, IHandle<ModuleSelectedEvent>, IHandle<TeamSetEvent>
	{
		const string PlayerA = "Player A";
		const string PlayerB = "Player B";

		private readonly IEventAggregator eventAggregator;
		private IPlayersModule PlayersModel { get; set; }
		private IDictionary<PlayerIndex, Player> Players { get; set; }
		private IDictionary<string, Player> PlayerAb { get; set; }

		private ObservableCollection<PlayerMetric> playerAbItems = new ObservableCollection<PlayerMetric>();
		public ObservableCollection<PlayerMetric> PlayerAbItems
		{
			get { return this.playerAbItems; }
			set
			{
				this.playerAbItems = value;
				NotifyOfPropertyChange(() => PlayerAbItems);
			}
		}

		private string playerAbString = PlayerA;
		public string PlayerAbString
		{
			get { return this.playerAbString; }
			set
			{
				this.playerAbString = value;
				NotifyOfPropertyChange(() => PlayerAbString);
			}
		}

		private Color playerAColour = default(Color);
		public Color PlayerAColour
		{
			get { return this.playerAColour; }
			set
			{
				this.playerAColour = value;
				NotifyOfPropertyChange(() => PlayerAColour);
			}
		}

		private Color playerBColour = default(Color);
		public Color PlayerBColour
		{
			get { return this.playerBColour; }
			set
			{
				this.playerBColour = value;
				NotifyOfPropertyChange(() => PlayerBColour);
			}
		}

		private string aTitle = string.Empty;
		public string ATitle
		{
			get { return this.aTitle; }
			set
			{
				this.aTitle = value;
				NotifyOfPropertyChange(() => ATitle);
			}
		}

		private string bTitle = string.Empty;
		public string BTitle
		{
			get { return this.bTitle; }
			set
			{
				this.bTitle = value;
				NotifyOfPropertyChange(() => BTitle);
			}
		}

		private Cm93.Model.Structures.Team team = default(Cm93.Model.Structures.Team);
		public Cm93.Model.Structures.Team Team
		{
			get { return this.team; }
			set
			{
				this.team = value;
				NotifyOfPropertyChange(() => Team);
			}
		}

		private string teamsLabel = string.Empty;
		public string TeamsLabel
		{
			get { return this.teamsLabel; }
			set
			{
				this.teamsLabel = value;
				NotifyOfPropertyChange(() => TeamsLabel);
			}
		}

		private bool showOnlyMyTeam = true;
		public bool ShowOnlyMyTeam
		{
			get { return this.showOnlyMyTeam; }
			set
			{
				this.showOnlyMyTeam = value;
				NotifyOfPropertyChange(() => ShowOnlyMyTeam);
			}
		}

		private ObservableCollection<PlayerFilter> playerFilters = new ObservableCollection<PlayerFilter>();
		public ObservableCollection<PlayerFilter> PlayerFilters
		{
			get { return this.playerFilters; }
			set
			{
				this.playerFilters = value;
				NotifyOfPropertyChange(() => PlayerFilters);
			}
		}

		private PlayerFilter selectedPlayerFilter = default(PlayerFilter);
		public PlayerFilter SelectedPlayerFilter
		{
			get { return this.selectedPlayerFilter; }
			set
			{
				this.selectedPlayerFilter = value;
				NotifyOfPropertyChange(() => SelectedPlayerFilter);
			}
		}

		private ObservableCollection<Position> positionFilters = new ObservableCollection<Position>();
		public ObservableCollection<Position> PositionFilters
		{
			get { return this.positionFilters; }
			set
			{
				this.positionFilters = value;
				NotifyOfPropertyChange(() => PositionFilters);
			}
		}

		private Position selectedPositionFilter = Position.All;
		public Position SelectedPositionFilter
		{
			get { return this.selectedPositionFilter; }
			set
			{
				this.selectedPositionFilter = value;
				UpdatePlayerGrid();

				NotifyOfPropertyChange(() => SelectedPlayerFilter);
			}
		}

		private PlayerRow selectedPlayer = default(PlayerRow);
		public PlayerRow SelectedPlayer
		{
			get { return this.selectedPlayer; }
			set
			{
				this.selectedPlayer = value;

				UpdatePlayerSelected();
				NotifyOfPropertyChange(() => SelectedPlayer);
				NotifyOfPropertyChange(() => IsPlayerSelected);
				NotifyOfPropertyChange(() => CanContractBidRelease);
			}
		}

		public bool IsPlayerSelected
		{
			get { return SelectedPlayer != null; }
		}

		private ObservableCollection<PlayerRow> playerGrid = new ObservableCollection<PlayerRow>();
		public ObservableCollection<PlayerRow> PlayerGrid
		{
			get { return this.playerGrid; }
			set
			{
				this.playerGrid = value;
				NotifyOfPropertyChange(() => PlayerGrid);
			}
		}

		private ObservableCollection<PlayerMetricRow> playerMetricGrid = new ObservableCollection<PlayerMetricRow>();
		public ObservableCollection<PlayerMetricRow> PlayerMetricGrid
		{
			get { return this.playerMetricGrid; }
			set
			{
				this.playerMetricGrid = value;
				NotifyOfPropertyChange(() => PlayerMetricGrid);
			}
		}

		private int playerNumber = default(int);
		public int PlayerNumber
		{
			get { return this.playerNumber; }
			set
			{
				this.playerNumber = value;
				NotifyOfPropertyChange(() => PlayerNumber);
			}
		}

		#region Bids

		private double bid = default(double);
		public double Bid
		{
			get { return this.bid; }
			set
			{
				this.bid = value;
				NotifyOfPropertyChange(() => Bid);
				NotifyOfPropertyChange(() => BidString);
			}
		}

		private double maxBidValue = default(double);
		public double MaxBidValue
		{
			get { return this.maxBidValue; }
			set
			{
				this.maxBidValue = value;
				NotifyOfPropertyChange(() => MaxBidValue);
			}
		}

		public string BidString
		{
			get { return string.Format(CultureInfo.CurrentCulture, "{0:c0}", Bid); }
		}

		public string Balance
		{
			get { return string.Format(CultureInfo.CurrentCulture, "{0:c0}", Team.Balance); }
		}

		public double NumericAvailable
		{
			get { return this.Team.Balance - PlayersModel.Simulator.TeamBids[this.Team].Sum(b => b.BidAmount); }
		}

		public string Available
		{
			get
			{
				return string.Format(CultureInfo.CurrentCulture, "{0:c0}", NumericAvailable);
			}
		}

		private string contractButtonLabel = string.Empty;
		public string ContractButtonLabel
		{
			get { return this.contractButtonLabel; }
			set
			{
				this.contractButtonLabel = value;
				NotifyOfPropertyChange(() => ContractButtonLabel);
			}
		}

		private bool shirtNumberVisible = default(bool);
		public bool ShirtNumberVisible
		{
			get { return this.shirtNumberVisible; }
			set
			{
				this.shirtNumberVisible = value;
				NotifyOfPropertyChange(() => ShirtNumberVisible);
			}
		}

		#endregion

		[ImportingConstructor]
		public PlayersViewModel(IEventAggregator eventAggregator)
		{
			this.eventAggregator = eventAggregator;
			this.ModuleType = ModuleType.Players;
			this.PlayerAb = new Dictionary<string, Player>();

			foreach (var filter in Enum.GetValues(typeof(PlayerFilter)).Cast<PlayerFilter>())
				this.PlayerFilters.Add(filter);

			foreach (var filter in Enum.GetValues(typeof(Position)).Cast<Position>())
				this.PositionFilters.Add(filter);

			this.eventAggregator.Subscribe(this);
		}

		public override void SetModel(IModule model)
		{
			PlayersModel = (IPlayersModule) model;
			Players = PlayersModel.Players.ToDictionary(p => p.Index);
		}

		public void Handle(ModuleSelectedEvent message)
		{
			if (message.Module != ModuleType.Players)
				return;

			Players = PlayersModel.Players.ToDictionary(p => p.Index);
			UpdatePlayerGrid();
			NotifyOfPropertyChange(() => Balance);
			NotifyOfPropertyChange(() => Available);
			NotifyOfPropertyChange(() => Team);
		}

		private void UpdatePlayerGrid()
		{
			this.playerGrid.Clear();

			foreach (var player in PlayersModel.Players.Where(p =>
				(SelectedPositionFilter == Position.All ||
				p.Positions.Contains(SelectedPositionFilter)) &&
				(!ShowOnlyMyTeam || p.Team.TeamName == Team.TeamName)))
				this.playerGrid.Add(new PlayerRow
					{
						Name = string.Format(CultureInfo.CurrentCulture, "{0}, {1}", player.LastName, player.FirstName),
						Number = player.Number,
						Age = player.Age,
						Goals = player.Goals,
						Positions = string.Join("\n", player.Positions),
						Rating = player.Rating,
						Team = player.Team.TeamName
					});

			NotifyOfPropertyChange(() => PlayerGrid);
		}

		private void UpdatePlayerSelected()
		{
			this.playerMetricGrid.Clear();

			if (SelectedPlayer == null)
				return;

			var player = Players[new PlayerIndex(SelectedPlayer.Number, SelectedPlayer.Team)];

			var playerMetricRows = PopulateMetricGrid(player);
			UpdateBidRelease(player, playerMetricRows);
			UpdateChart(player);

			foreach (var playerMetricRow in playerMetricRows.OrderBy(r => r.Order))
				this.playerMetricGrid.Add(playerMetricRow);

			NotifyOfPropertyChange(() => PlayerMetricGrid);
		}

		private void UpdateChart(Player player)
		{
			this.playerAbItems.Clear();

			PlayerAb[PlayerAbString] = player;

			if (PlayerAb.Count != 2)
			{
				InitialiseChart(player);
				return;
			}

			ATitle = PlayerAb[PlayerA].LastName;
			BTitle = PlayerAb[PlayerB].LastName;

			PlayerAColour = PlayerAb[PlayerA].Team.PrimaryColour;
			PlayerBColour = PlayerAb[PlayerB].Team.PrimaryColour;

			var playerAMetrics = PopulateMetricGrid(PlayerAb[PlayerA]);
			var playerBMetrics = PopulateMetricGrid(PlayerAb[PlayerB]);

			PlayerAbItems = new ObservableCollection<PlayerMetric>();

			foreach (var metrics in playerAMetrics.Zip(playerBMetrics, (a, b) => new { A = a, B = b }))
			{
				if (metrics.A.Attribute == "Number")
					continue;

				double aVal, bVal;

				if (!Double.TryParse(metrics.A.Value, out aVal) || !Double.TryParse(metrics.B.Value, out bVal))
					continue;

				this.playerAbItems.Add(new PlayerMetric { Label = metrics.A.Attribute, PlayerA = aVal, PlayerB = bVal });
			}

			NotifyOfPropertyChange(() => PlayerAbItems);
		}

		//	The OxyPlot library doesn't render the chart correctly first time. By populating the Attribute
		//	names before there are two proper data points, the error is bypassed.
		private void InitialiseChart(Player player)
		{
			if (PlayerAbString == PlayerA)
				ATitle = player.LastName;
			else
				BTitle = player.LastName;

			var playerMetrics = PopulateMetricGrid(player);

			PlayerAbItems = new ObservableCollection<PlayerMetric>();

			foreach (var metrics in playerMetrics)
			{
				if (metrics.Attribute == "Number")
					continue;

				double val;

				if (!Double.TryParse(metrics.Value, out val))
					continue;

				this.playerAbItems.Add(new PlayerMetric { Label = metrics.Attribute });
			}

			NotifyOfPropertyChange(() => PlayerAbItems);
		}

		private void UpdateBidRelease(Player player, ICollection<PlayerMetricRow> playerMetricRows)
		{
			MaxBidValue = player.Team == Team ? player.NumericValue * 3 : Math.Min(player.NumericValue * 2, NumericAvailable);
			Bid = player.Team == Team ? player.ReleaseValue : Math.Min(player.NumericValue, NumericAvailable);

			if (player.Team == Team)
			{
				ShirtNumberVisible = false;
				ContractButtonLabel = "Release";
				playerMetricRows.Add(new PlayerMetricRow
					{
						Order = playerMetricRows.Count + 1,
						Attribute = "Release",
						Value = string.Format(CultureInfo.CurrentCulture, "{0:c0}", player.ReleaseValue)
					});
			}
			else
			{
				ShirtNumberVisible = true;
				ContractButtonLabel = "Bid";
				var smallestFreeNumber = 1;
				while (Team.Players.Any(p => p.Number == smallestFreeNumber) ||
					PlayersModel.Simulator.TeamBids[Team].Any(b => b.PlayerNumber == smallestFreeNumber))
					++smallestFreeNumber;
				PlayerNumber = smallestFreeNumber;

				var previousBid = PlayersModel.Simulator.TeamBids[Team].SingleOrDefault(b => b.Player == player);

				if (previousBid != null)
				{
					Bid = previousBid.BidAmount;
					MaxBidValue = previousBid.BidAmount;
				}
			}
		}

		private static IList<PlayerMetricRow> PopulateMetricGrid(Player player)
		{
			var properties = player.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			var playerMetricRows = new List<PlayerMetricRow>();

			foreach (var propertyDefinition in properties)
			{
				if (!propertyDefinition.IsDefined(typeof(PlayerMetricAttribute), true))
					continue;

				var propertyValue = propertyDefinition.GetValue(player, null);
				var attribute = propertyDefinition.GetAttributes<PlayerMetricAttribute>(false).Single();

				var propertyString = propertyValue is ICollection
					? string.Join("\n", ((ICollection) propertyValue).
						Cast<object>().
						Select(o => o.ToString()).
						OrderBy(s => s))
					: propertyValue.ToString();

				playerMetricRows.Add(new PlayerMetricRow
					{
						Order = attribute.Order,
						Attribute = propertyDefinition.Name,
						Value = propertyString
					});
			}

			return playerMetricRows;
		}

		public void Handle(TeamSetEvent message)
		{
			Team = message.Team;
			TeamsLabel = Team.TeamName;
			ShowOnlyMyTeam = true;

			UpdatePlayerGrid();
		}

		public bool CanToggleTeams
		{
			get { return true; }
		}

		public void ToggleTeams()
		{
			TeamsLabel = ShowOnlyMyTeam ? "All Teams" : Team.TeamName;
			ShowOnlyMyTeam = !ShowOnlyMyTeam;

			UpdatePlayerGrid();
		}

		public bool CanContractBidRelease
		{
			get
			{
				if (SelectedPlayer == null || Bid == 0)
					return false;

				var player = Players[new PlayerIndex(SelectedPlayer.Number, SelectedPlayer.Team)];

				return player.Team == Team || PlayersModel.Simulator.TeamBids[Team].All(b => b.Player != player);
			}
		}

		public void ContractBidRelease()
		{
			var player = Players[new PlayerIndex(SelectedPlayer.Number, SelectedPlayer.Team)];

			if (player.Team == Team)
			{
				player.ReleaseValue = (int) Bid;
				UpdatePlayerSelected();
				return;
			}

			var playerBid = new Bid { BidAmount = (int) Bid, Player = player, PlayerNumber = PlayerNumber, PurchasingTeam = Team };

			PlayersModel.Simulator.SubmitBid(playerBid);

			NotifyOfPropertyChange(() => CanContractBidRelease);
			NotifyOfPropertyChange(() => Available);
		}

		public bool CanUpPlayerNumber
		{
			get { return true; }
		}

		public void UpPlayerNumber()
		{
			if (PlayerNumber >= 49)
				return;

			var raisedPlayerNumber = PlayerNumber + 1;

			while ((Team.Players.Any(p => p.Number == raisedPlayerNumber) ||
				PlayersModel.Simulator.TeamBids[Team].Any(b => b.PlayerNumber == raisedPlayerNumber)) &&
				raisedPlayerNumber < 50)
				++raisedPlayerNumber;

			if (raisedPlayerNumber < 50)
				PlayerNumber = raisedPlayerNumber;
		}

		public bool CanDownPlayerNumber
		{
			get { return true; }
		}

		public void DownPlayerNumber()
		{
			if (PlayerNumber <= 1)
				return;

			var loweredPlayerNumber = PlayerNumber - 1;

			while ((Team.Players.Any(p => p.Number == loweredPlayerNumber) ||
				PlayersModel.Simulator.TeamBids[Team].Any(b => b.PlayerNumber == loweredPlayerNumber)) &&
				loweredPlayerNumber > 0)
				--loweredPlayerNumber;

			if (loweredPlayerNumber > 0)
				PlayerNumber = loweredPlayerNumber;
		}

		public bool CanFlipAb
		{
			get { return true; }
		}

		public void FlipAb()
		{
			PlayerAbString = PlayerAbString == PlayerA ? PlayerB : PlayerA;
		}
	}
}