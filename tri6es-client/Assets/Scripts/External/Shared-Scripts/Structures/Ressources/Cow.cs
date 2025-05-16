using Shared.DataTypes;
using System.Collections.Generic;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    class Cow : Ressource
    {
        public override int MaxProgress => Constants.HoursToGameTicks(4);

        public override int harvestReduction => Constants.HoursToGameTicks(2);

        public override RessourceType ressourceType => RessourceType.COW;
        public override Dictionary<string, int> Weather => new Dictionary<string, int>();


        public Cow() : base()
        {

        }

        public Cow(HexCell cell) : base(cell)
        {

        }

        public Cow(HexCell Cell, int Progress) : base(Cell, Progress)
        {

        }
    }
}
