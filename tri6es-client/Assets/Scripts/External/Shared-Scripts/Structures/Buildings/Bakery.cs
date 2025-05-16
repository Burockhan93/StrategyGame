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
    class Bakery : RefineryBuilding
    {

        public override string description => "The Bakery is used to process Wheat and Wood into Food.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.WHEAT, new [] { 6, 12, 26 } },
            { RessourceType.WOOD, new [] { 6, 12, 26 } },
            { RessourceType.FOOD, new [] { 6, 12, 26 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(180),
            Constants.MinutesToGameTicks(120),
            Constants.MinutesToGameTicks(50)
        };

        public override Dictionary<RessourceType, int> InputRecipe => new Dictionary<RessourceType, int> { { RessourceType.WHEAT, 2 }, { RessourceType.WOOD, 1 } };
        public override Dictionary<RessourceType, int> OutputRecipe => new Dictionary<RessourceType, int> { { RessourceType.FOOD, 1 } };


        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 5 }, { RessourceType.STONE, 2 }  },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6 }, { RessourceType.STONE, 4 }, { RessourceType.LEATHER, 4 }  },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10 }, { RessourceType.STONE, 5 }, { RessourceType.IRON, 4 } }
                };
                return result;
            }
        }

        public Bakery() : base()
        {

        }

        public Bakery(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress
            ) : base(Cell, Tribe, Level, Inventory, Progress)
        {

        }

        public override bool IsPlaceableAt(HexCell cell)
        {
            return base.IsPlaceableAt(cell) ;
        }
    }
}
