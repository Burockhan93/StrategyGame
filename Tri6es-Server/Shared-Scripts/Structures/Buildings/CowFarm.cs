using System.ComponentModel;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    [Description("Cow Farm")]
    public class CowFarm : ProductionBuilding
    {
        public override string description => "The Cowfarm is used to get Cows from a nearby Cowressource. The Cowfarm needs to be placed adjacent to atleast one Cow. More adjacent Cows will improve the efficiency of the farm.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.COW, new [] { 4, 10, 20 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(200),
            Constants.MinutesToGameTicks(100),
            Constants.MinutesToGameTicks(50)
        };

        public override RessourceType ProductionType => RessourceType.COW;

        public override byte Gain => 1;



        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 4 }, { RessourceType.WHEAT, 2 }},
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6}, { RessourceType.STONE, 2 }, { RessourceType.WHEAT, 5 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10}, { RessourceType.STONE, 4 }, { RessourceType.IRON, 4 } }
                };
                return result;
            }
        }

        public CowFarm() : base() {}

        public CowFarm(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
            ) : base(Cell, Tribe, Level, Inventory, Progress) {}
    }
}
