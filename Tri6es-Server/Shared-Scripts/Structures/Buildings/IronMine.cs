using System.ComponentModel;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    [Description("Iron Mine")]
    class IronMine : ProductionBuilding
    {
        public override string description => "The Ironmine is used to produce Iron from a nearby Stone ressource. The Ironmine needs to be placed adjacent to atleast one Stone ressource. More adjacent Stone ressources will improve the efficiency of the mine.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.IRON, new [] { 4, 10, 20 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(10),
            Constants.MinutesToGameTicks(10),
            Constants.MinutesToGameTicks(10)
        };

        public override RessourceType ProductionType => RessourceType.IRON;

        public override byte Gain => 1;

        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8 }, { RessourceType.STONE, 4 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8}, { RessourceType.STONE, 2 }, { RessourceType.IRON, 4 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8}, { RessourceType.STONE, 5 }, { RessourceType.IRON, 5 } }
                };
                return result;
            }
        }

        public IronMine() : base() {}

        public IronMine (
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
        ) : base(Cell, Tribe, Level, Inventory, Progress) {}
    }
}
