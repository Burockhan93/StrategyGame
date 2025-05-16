using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class KnightBuff : TroopBuff
    {
        public override CollectableType type => CollectableType.KNIGHTBUFF;
        public override TroopType ttype => TroopType.KNIGHT;

        public KnightBuff() : base()
        {

        }
        public KnightBuff(HexCell cell) : base(cell)
        {

        }
        public KnightBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}
