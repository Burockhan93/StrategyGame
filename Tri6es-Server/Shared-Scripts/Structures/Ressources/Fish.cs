using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Communication;

namespace Shared.Structures
{
    public class Fish : Ressource
    {
        public override int MaxProgress => Constants.HoursToGameTicks(1);
        public override RessourceType ressourceType => RessourceType.FOOD;
        public override int harvestReduction => Constants.MinutesToGameTicks(30);

        public Fish() : base()
        {

        }

        public Fish(HexCell cell) : base(cell)
        {

        }

        public Fish(HexCell Cell, int Progress) : base(Cell, Progress)
        {
            
        }
    }
}
