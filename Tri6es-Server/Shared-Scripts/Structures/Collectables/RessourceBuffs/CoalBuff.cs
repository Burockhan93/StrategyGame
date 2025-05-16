using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class CoalBuff : RessourceBuff
    {

        public override CollectableType type => CollectableType.COALBUFF;
        public override RessourceType rtype => RessourceType.COAL;


        public CoalBuff() : base()
        {

        }
        public CoalBuff(HexCell cell) : base(cell)
        {

        }
        public CoalBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}
