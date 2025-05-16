using System;
using Shared.HexGrid;
using Shared.DataTypes;
using System.Collections.Generic;


namespace Shared.Structures
{
	/// <summary>A destroyed building of any type.</summary>
	public class Ruin : Building
	{
		/// <summary>The building type this used to be.</summary>
		public Type BuildingType;

		public override string description => "This building has been destroyed, by repairing it you can use it again.";

		public override byte MaxLevel => 4;

		public override Dictionary<RessourceType, int>[] Recipes
			{
				get
					{
						Dictionary<RessourceType, int>[] result = {
							new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 5 } },
							new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 5 } },
							new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 5 } },
							new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 5 } },
						};
					return result;
				}
			}

		public Ruin(HexCell Cell, byte Tribe, byte Level, Type building) : base(Cell, Tribe, Level)
		{
			BuildingType = building;
		}

		public override void DoTick() {}
	}
}
