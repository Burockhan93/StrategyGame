using Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.HexGrid;
using UnityEngine;
using Shared.Communication;

namespace Shared.Structures
{
    class Tanner: RefineryBuilding
    {
        public override string description => "The Tanner is used to process Cows into Leather.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.COW, new [] { 6, 12, 26 } },
            { RessourceType.LEATHER, new [] { 6, 12, 26 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(200),
            Constants.MinutesToGameTicks(100),
            Constants.MinutesToGameTicks(50)
        };

        public override Dictionary<RessourceType, int> InputRecipe => new Dictionary<RessourceType, int> { { RessourceType.COW, 1 } };
        public override Dictionary<RessourceType, int> OutputRecipe => new Dictionary<RessourceType, int> { { RessourceType.LEATHER, 1 } };


        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6 }, { RessourceType.STONE, 4 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8 }, { RessourceType.STONE, 4 }, { RessourceType.IRON, 2 }, },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 12 }, { RessourceType.STONE, 6 }, { RessourceType.IRON, 5 }, }
                };
                return result;
            }
        }

        public Tanner() : base() {}

        public Tanner(
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
