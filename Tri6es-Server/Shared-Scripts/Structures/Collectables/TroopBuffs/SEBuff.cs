using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class SEBuff : TroopBuff
    {

        public override CollectableType type => CollectableType.SEBUFF;
        public override TroopType ttype => TroopType.SIEGE_ENGINE;

        public SEBuff() : base()
        {

        }
        public SEBuff(HexCell cell) : base(cell)
        {

        }
        public SEBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}
