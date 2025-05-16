using Shared.HexGrid;
using Shared.DataTypes;

namespace Shared.Structures
{
    public class Grass : Ressource
    {
        public override int MaxProgress => 4;
        public override RessourceType ressourceType => RessourceType.WHEAT;
        public override int harvestReduction => 2;

        public Grass() : base()
        {

        }

        public Grass(HexCell cell) : base(cell)
        {

        }

        public Grass(HexCell Cell, int Progress) : base(Cell, Progress)
        {
            
        }

        public override void DoTick() 
        {
            base.DoTick();
        }

        public override bool Harvestable()
        {
            return false;
        }

        public override bool ManuallyHarvestable()
        {
            return false;
        }
    }
}
