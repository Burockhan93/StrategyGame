using System.ComponentModel;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    [Description("Coal Ore")]
    class CoalOre : Ressource
    {
        public override int MaxProgress => Constants.HoursToGameTicks(4);
        public override RessourceType ressourceType => RessourceType.COAL;
        public override int harvestReduction => Constants.HoursToGameTicks(3);

        public CoalOre() : base()
        {

        }

        public CoalOre(HexCell cell) : base(cell)
        {

        }

        public CoalOre(HexCell Cell, int Progress) : base(Cell, Progress)
        {

        }
    }
}
