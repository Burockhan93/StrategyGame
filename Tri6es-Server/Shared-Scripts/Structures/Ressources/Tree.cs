using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Communication;

namespace Shared.Structures
{
    public class Tree : Ressource
    {
        public override int MaxProgress => Constants.HoursToGameTicks(4);
        public override RessourceType ressourceType => RessourceType.WOOD;
        public override int harvestReduction => Constants.HoursToGameTicks(1);

        public Tree() : base()
        {
            
        }

        public Tree(HexCell cell) : base(cell)
        {

        }

        public Tree(HexCell Cell, int Progress) : base(Cell, Progress)
        {
            
        }
    }
}
