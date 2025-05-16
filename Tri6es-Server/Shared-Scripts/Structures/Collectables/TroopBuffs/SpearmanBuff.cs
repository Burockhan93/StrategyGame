using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class SpearmanBuff : TroopBuff
    {

        public override CollectableType type => CollectableType.SPEARMANBUFF;
        public override TroopType ttype => TroopType.SPEARMAN;

        public SpearmanBuff() : base()
        {

        }
        public SpearmanBuff(HexCell cell) : base(cell)
        {

        }
        public SpearmanBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}
