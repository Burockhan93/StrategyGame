using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class CowBuff : RessourceBuff
    {

        public override CollectableType type => CollectableType.COWBUFF;
        public override RessourceType rtype => RessourceType.COW;


        public CowBuff() : base()
        {

        }
        public CowBuff(HexCell cell) : base(cell)
        {

        }
        public CowBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}

