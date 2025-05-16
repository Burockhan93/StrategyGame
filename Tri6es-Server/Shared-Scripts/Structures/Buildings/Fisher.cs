using Shared.DataTypes;
using System.Collections.Generic;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    class Fisher : ProductionBuilding
    {
        public override string description => "The Fisher is used to gain Food from a nearby Fishressource. The Fisher needs to be placed adjacent to Fishressource. More adjacent Fish will impove the efficiency of the Fisher.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.FOOD, new [] { 4, 4, 4 } },
        };

        public override RessourceType ProductionType => RessourceType.FOOD;

        public override byte Gain => 1;

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(60),
            Constants.MinutesToGameTicks(25),
            Constants.MinutesToGameTicks(12)
        };

        private const int elevationThreshold = 40;

        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6}, { RessourceType.STONE, 2} },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6}, { RessourceType.STONE, 4}, { RessourceType.LEATHER, 2 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8}, { RessourceType.STONE, 4 }, { RessourceType.IRON, 2 } }
                };
                return result;
            }
        }

        public Fisher() : base() {}

        public Fisher(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
        ) : base(Cell, Tribe, Level, Inventory, Progress) {}
    }
}