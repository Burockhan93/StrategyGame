using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class WheatBuff : RessourceBuff
    {

        public override CollectableType type => CollectableType.WHEATBUFF;
        public override RessourceType rtype => RessourceType.WHEAT;


        public WheatBuff() : base()
        {

        }
        public WheatBuff(HexCell cell) : base(cell)
        {

        }
        public WheatBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}

