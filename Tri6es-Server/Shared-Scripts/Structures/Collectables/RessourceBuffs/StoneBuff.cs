using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using System.ComponentModel;

namespace Shared.Structures
{
    [Description("Chest")]
    class StoneBuff : RessourceBuff
    {
        public override CollectableType type => CollectableType.STONEBUFF;
        public override RessourceType rtype => RessourceType.STONE;


        public StoneBuff() : base()
        {

        }
        public StoneBuff(HexCell cell) : base(cell)
        {

        }
        public StoneBuff(HexCell cell, int time) : base(cell)
        {

        }




    }
}