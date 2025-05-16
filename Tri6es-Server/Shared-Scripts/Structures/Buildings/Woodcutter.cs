using Shared.DataTypes;
using System.Collections.Generic;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    public class Woodcutter : ProductionBuilding
    {
        public override string description => "The Woodcutter is used to produce Wood from nearby Woodressources. The Woodcutter needs to be placed adjacent atleast one Tree or Bush. More adjacent Trees or Bushes will increase the efficiency and Trees are more efficient than Bushes.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.WOOD, new [] { 4, 8, 12 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(90),
            Constants.MinutesToGameTicks(50),
            Constants.MinutesToGameTicks(20)
        };

        public override RessourceType ProductionType => RessourceType.WOOD;

        public override byte Gain => 1;



        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 2 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 5 }, { RessourceType.STONE, 2 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8 }, { RessourceType.STONE, 4 }, { RessourceType.IRON, 2 } }
                };
                return result;
            }
        }

        public Woodcutter() : base() {}

        public Woodcutter(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
        ) : base(Cell, Tribe, Level, Inventory, Progress) {}
    }
}
