using System.ComponentModel;
using System.Collections.Generic;
using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    using RessourceMap = Dictionary<RessourceType, int>;

    [Description("Armor Smith")]
    class ArmorSmith : MultiRefineryBuilding
    {
        public override string description => "The Armor Smith is used to create armors for troops.";

        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.IRON, new [] { 10, 20, 40 } },
            { RessourceType.LEATHER, new [] { 10, 20, 40 } },
            { RessourceType.IRON_ARMOR, new [] { 8, 14, 20 } },
            { RessourceType.LEATHER_ARMOR, new [] { 8, 14, 20 } },
        };

        public override RessourceMap[] Recipes => new [] {
            new RessourceMap { { RessourceType.WOOD, 5 }, { RessourceType.STONE, 2 } },
            new RessourceMap { { RessourceType.WOOD, 5 }, { RessourceType.STONE, 5} },
            new RessourceMap { { RessourceType.WOOD, 10 }, { RessourceType.STONE, 10 }, { RessourceType.IRON, 5} }
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(1),
            Constants.MinutesToGameTicks(1),
            Constants.MinutesToGameTicks(1),
        };

        protected override Dictionary<RessourceType, RessourceMap> AvailableRecipes => new Dictionary<RessourceType, RessourceMap> {
            { RessourceType.IRON_ARMOR, new RessourceMap { { RessourceType.IRON, 2 }, { RessourceType.LEATHER, 1 } } },
            { RessourceType.LEATHER_ARMOR, new RessourceMap { { RessourceType.LEATHER, 3 } } },
        };

        public ArmorSmith() : base() {}

        public ArmorSmith(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            int progress,
            RessourceType output
        ) : base(cell, tribe, level, inventory, progress, output) {}
    }
}
