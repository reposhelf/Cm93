﻿/*
        Copyright © Iain McDonald 2013-2016
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
using Cm93.Model.Enumerations;
using Cm93.Model.Structures;

namespace Cm93.Model.Interfaces
{
	public interface IFixture
	{
		Team TeamHome { get; set; }
		Team TeamAway { get; set; }
		string CompetitionName { get; set; }

		int Week { get; }

		int GoalsHome { get; set; }
		int GoalsAway { get; set; }
		int ChancesHome { get; set; }
		int ChancesAway { get; set; }

		int MinutesAddedOn { get; set; }
		int Minutes { get; set; }
		PlayingPeriod PlayingPeriod { get; set; }

		Fixture DependentHome { get; set; }
		Fixture DependentAway { get; set; }
	}
}
