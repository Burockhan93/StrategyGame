using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class WoodBuff : RessourceBuff
    {
        public override CollectableType type => CollectableType.WOODBUFF;
        public override RessourceType rtype => RessourceType.WOOD;


        public WoodBuff() : base()
        {

        }
        public WoodBuff(HexCell cell) : base(cell)
        {

        }
        public WoodBuff(HexCell cell, int time) : base(cell)
        {

        }




    }
}
