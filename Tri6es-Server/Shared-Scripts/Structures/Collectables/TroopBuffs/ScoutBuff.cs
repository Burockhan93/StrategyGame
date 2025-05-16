using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class ScoutBuff : TroopBuff
    {

        public override CollectableType type => CollectableType.SEBUFF;
        public override TroopType ttype => TroopType.SCOUT;

        public ScoutBuff() : base()
        {

        }
        public ScoutBuff(HexCell cell) : base(cell)
        {

        }
        public ScoutBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}
