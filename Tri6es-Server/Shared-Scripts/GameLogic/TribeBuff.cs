using System;
using Shared.Communication;
using Shared.DataTypes;
using Shared.Structures;
using Shared.HexGrid;

namespace Shared.Game
{
    public class TribeBuff
    {
        public RessourceType rtype;
        public TroopType ttype;
        public int ctime;
        public int maxTime = Constants.HoursToGameTicks(1);
        public int ticks;
        public Boolean isRessource = false;
        public Boolean isTroop = false;

        public TribeBuff(RessourceType ctype)
        {
            rtype = ctype;
            ctime = maxTime;
            isRessource = true;
        }

        public TribeBuff(TroopType ctype)
        {
            ttype = ctype;
            ctime = maxTime;
            isTroop = true;
        }

        public void DoTick()
        {
            ctime--;
            ticks++;
        }
    }
}