using Shared.DataTypes;
using System.Collections.Generic;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    class Butcher: RefineryBuilding
    {
        public override string description => "The Butcher is used to process Cows into Food.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.COW, new [] { 6, 8, 12 } },
            { RessourceType.FOOD, new [] { 6, 8, 12 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(200),
            Constants.MinutesToGameTicks(100),
            Constants.MinutesToGameTicks(50)
        };
        public override Dictionary<RessourceType, int> InputRecipe => new Dictionary<RessourceType, int> { { RessourceType.COW, 1 } };
        public override Dictionary<RessourceType, int> OutputRecipe => new Dictionary<RessourceType, int> { { RessourceType.FOOD, 1 } };


        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6 }, { RessourceType.STONE, 4 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6 }, { RessourceType.STONE, 8 }, { RessourceType.IRON, 1 }, },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6 }, { RessourceType.STONE, 12 }, { RessourceType.IRON, 4 }, }
                };
                return result;
            }
        }

        public Butcher() : base() {}

        public Butcher(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
        ) : base(Cell, Tribe, Level, Inventory, Progress) {}

        public override bool IsPlaceableAt(HexCell cell)
        {
            return base.IsPlaceableAt(cell) ;
        }
    }
}
