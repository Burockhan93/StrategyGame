using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    using RessourceMap = Dictionary<RessourceType, int>;
    public class Workshop: TroopProductionBuilding
    {
        public override string description => "The Workshop is used to construct Siege Engines, which are a type of Troop specialized in attacking enemy buildings.";

        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.WOOD, new [] { 12, 12, 24 } },
            { RessourceType.IRON, new [] { 12, 12, 24 } },
        };

        public override int[] MaxProgresses => new int[]
        {
            Constants.MinutesToGameTicks(480),
            Constants.MinutesToGameTicks(300),
            Constants.MinutesToGameTicks(120)
        };

        public override RessourceMap[] Recipes => new [] {
            new RessourceMap { { RessourceType.WOOD, 5 }, { RessourceType.STONE, 2 } },
            new RessourceMap { { RessourceType.WOOD, 5 }, { RessourceType.STONE, 5} },
            new RessourceMap { { RessourceType.WOOD, 10 }, { RessourceType.STONE, 10 }, { RessourceType.IRON, 5} }
        };

        public override Dictionary<RessourceType, int> InputRecipe => new Dictionary<RessourceType, int> { { RessourceType.WOOD, 2 }, { RessourceType.IRON, 2 } };

        public override TroopType OutputTroop => TroopType.SIEGE_ENGINE;

        public Workshop() : base() {}

        public Workshop(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            int progress,
            TroopInventory troops
        ) : base(cell, tribe, level, inventory, progress, troops) {}
    }
}
