using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    class Wheat : Ressource
    {
        public override int MaxProgress => Constants.HoursToGameTicks(2);

        public override int harvestReduction => Constants.HoursToGameTicks(1);

        public override RessourceType ressourceType => RessourceType.WHEAT;

        public Wheat() : base()
        {

        }

        public Wheat(HexCell cell) : base(cell)
        {

        }

        public Wheat(HexCell Cell, int Progress) : base(Cell, Progress)
        {

        }
    }
}
