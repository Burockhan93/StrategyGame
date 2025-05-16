using System.ComponentModel;
using System.Collections.Generic;
using Shared.HexGrid;
using Shared.DataTypes;

namespace Shared.Structures
{
    [Description("Road")]
    class LandRoad : Road
    {
        public override string description => "The Landroad can be used to connect Buildings with each other so that they can exchange Ressources.";

        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 2 }},
                    new Dictionary<RessourceType, int>{ { RessourceType.STONE, 4 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 2 }, { RessourceType.IRON, 1 } }
                };
                return result;
            }
        }

        public LandRoad() : base() {}

        public LandRoad(
            HexCell Cell,
            byte Tribe,
            byte Level
        ) : base(Cell, Tribe, Level) {}

        public override bool IsPlaceableAt(HexCell cell)
        {
            if (cell.Data.Biome == HexCellBiome.WATER)
                return false;
            return base.IsPlaceableAt(cell);
        }

        public override void DoTick()
        {
            base.DoTick();
        }
    }
}
