using System.ComponentModel;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Communication;
using Shared.Game;
using System;

namespace Shared.Structures
{
    [Description("Truhe")]
    public abstract class TroopBuff : Collectable
    {
        public int ctime;
        public int maxTime = Constants.HoursToGameTicks(1);
        public abstract TroopType ttype { get; }
        public Boolean isActive;



        public TroopBuff() : base()
        {

        }
        public TroopBuff(HexCell cell) : base(cell)
        {

        }
        public TroopBuff(HexCell cell, int time) : base(cell, time)
        {

        }

        public override int Collect(Tribe tribe, Player player)
        {
            isActive = true;
            tribe.AddBuff(this);
            da = 0;
            //ctime = maxTime;

            return ctime;
        }
        public override void DoTick()
        {
            if (ctime != maxTime)
            {
                ctime++;
            }
            else
            {
                isActive = false;

            }
        }


    }
}
