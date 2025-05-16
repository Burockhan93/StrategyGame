using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Communication;

namespace Shared.Structures
{
    class Quarry : ProductionBuilding
    {
        public override string description => "The Quarry is used to produce Stone from nearby Rocks. The Quarry needs to be placed adjacent to atleast one Rock. More adjacent Rocks will improve the efficiency of the Quarry.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.STONE, new [] { 4, 10, 20 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(180),
            Constants.MinutesToGameTicks(120),
            Constants.MinutesToGameTicks(60),
        };

        public override RessourceType ProductionType => RessourceType.STONE;
        public override byte Gain => 1;

        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 5 } },
                    new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 10 }, { RessourceType.LEATHER, 2 } },
                    new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 10 }, { RessourceType.LEATHER, 2 }, { RessourceType.IRON, 4 } }
                };
                return result;
            }
        }

        public Quarry() : base() {}

        public Quarry(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
        ) : base(Cell, Tribe, Level, Inventory, Progress) {}
    }
}
