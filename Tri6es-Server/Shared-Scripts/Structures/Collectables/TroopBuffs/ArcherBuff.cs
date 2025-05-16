using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class ArcherBuff : TroopBuff
    {

        public override CollectableType type => CollectableType.ARCHERBUFF;
        public override TroopType ttype => TroopType.ARCHER;

        public ArcherBuff() : base()
        {

        }
        public ArcherBuff(HexCell cell) : base(cell)
        {

        }
        public ArcherBuff(HexCell cell, int time) : base(cell)
        {

        }
    }
}
