using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    public class Bridge : Road
    {
        public override string description => "The Bridge can be used to connect buildings so that those can transfer ressources between each other. A bridge needs to be placed on water.";
        public override byte MaxLevel => 3;

        private int BridgeHeight = 2;

        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 5 }},
                    new Dictionary<RessourceType, int>{ { RessourceType.STONE, 4 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 5 }, { RessourceType.IRON, 2 } }
                };
                return result;
            }
        }

        public Bridge() : base()
        {

        }

        public Bridge(
            HexCell Cell,
            byte Tribe,
            byte Level
        ) : base(Cell, Tribe, Level) {}

        public override bool IsPlaceableAt(HexCell cell)
        {
            if(cell.Data.Biome != HexCellBiome.WATER)
            {
                return false;
            }
            return base.IsPlaceableAt(cell);
        }

        public override void DoTick()
        {
            base.DoTick();
        }

        public override bool HasBuilding(HexDirection direction)
        {
            return HasRoad(direction);
        }

        public override int GetElevation()
        {
            return this.Cell.Data.Elevation + this.BridgeHeight;
        }
    }
}
