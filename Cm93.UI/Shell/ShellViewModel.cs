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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Cm93.Model.Modules;
using Cm93.UI.Events;
using Cm93.UI.Modules;

namespace Cm93.UI.Shell
{
	[Export(typeof(ShellViewModel))]
	public class ShellViewModel : Conductor<ModuleViewModelBase>.Collection.OneActive, IHandle<ModuleSelectedEvent>, IHandle<TeamSetEvent>
	{
		private readonly IEventAggregator eventAggregator;
		private readonly IDictionary<ModuleType, ModuleViewModelBase> children;

		private bool teamSelected = false;
		public bool TeamSelected
		{
			get { return teamSelected; }
			set
			{
				teamSelected = value;
				NotifyOfPropertyChange(() => TeamSelected);
			}
		}

		[ImportingConstructor]
		public ShellViewModel(IEventAggregator eventAggregator,
			[ImportMany(typeof(ModuleViewModelBase))] IEnumerable<ModuleViewModelBase> children, ICreateModel model)
		{
			this.eventAggregator = eventAggregator;

			this.children = children.ToDictionary(c => c.ModuleType);

			foreach (var modelViewModel in this.children.
				Join(model.Modules,
					kvp => kvp.Key,
					kvp => kvp.Key,
					(vm, m) => new { ViewModel = vm.Value, Model = m.Value }))
			{
				modelViewModel.ViewModel.SetModel(modelViewModel.Model);
			}

			this.ActiveItem = this.children[ModuleType.SelectTeam];

			this.eventAggregator.Subscribe(this);
		}

		public void Handle(ModuleSelectedEvent message)
		{
			this.ActiveItem = this.children[message.Module];
		}

		public bool CanTeam()
		{
			return true;
		}

		public void Team()
		{
			this.eventAggregator.Publish(new ModuleSelectedEvent(ModuleType.Team));
		}

		public bool CanPlayers()
		{
			return true;
		}

		public void Players()
		{
			this.eventAggregator.Publish(new ModuleSelectedEvent(ModuleType.Players));
		}

		public bool CanFixtures()
		{
			return true;
		}

		public void Fixtures()
		{
			this.eventAggregator.Publish(new ModuleSelectedEvent(ModuleType.Fixtures));
		}

		public bool CanMatch()
		{
			return true;
		}

		public void Match()
		{
			this.eventAggregator.Publish(new ModuleSelectedEvent(ModuleType.Match));
		}

		public bool CanCompetitions()
		{
			return true;
		}

		public void Competitions()
		{
			this.eventAggregator.Publish(new ModuleSelectedEvent(ModuleType.Competitions));
		}

		public void Handle(TeamSetEvent message)
		{
			TeamSelected = !string.IsNullOrEmpty(message.Team.TeamName);
		}
	}
}
