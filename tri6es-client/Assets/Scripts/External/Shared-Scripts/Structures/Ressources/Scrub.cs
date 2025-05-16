using System.Collections.Generic;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Communication;

namespace Shared.Structures
{
    public class Scrub : Ressource
    {
        public override int MaxProgress => Constants.HoursToGameTicks(4);
        public override int harvestReduction => Constants.HoursToGameTicks(4);
        public override RessourceType ressourceType => RessourceType.WOOD;
        public override Dictionary<string, int> Weather => new Dictionary<string, int>();


        public Scrub() : base()
        {

        }

        public Scrub(HexCell cell) : base(cell)
        {

        }

        public Scrub(HexCell Cell, int Progress) : base(Cell, Progress)
        {
            
        }
    }
}
