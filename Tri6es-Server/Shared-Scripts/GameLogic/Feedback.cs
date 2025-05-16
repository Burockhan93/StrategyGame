using System;
using Shared.DataTypes;
using Shared.Structures;
using Shared.HexGrid;
using System.Collections.Generic;

namespace Shared.Game
{
    public class Feedback
    {
        public int quantity;
        public RessourceType resource;
        public String message;
        public HexCoordinates coordinates;
        public Type type;
        public Building building;
        public bool successfull;
        public BattleLog battleLog;
        public string playername;
        public byte TribeId;

        public enum FeedbackStyle { ui,harvest,build,upgrade,destroy,collect,battle,plainMessage }
        public FeedbackStyle feedbackStyle;

        public Feedback(FeedbackStyle style)
        {
            feedbackStyle = style;
            message = "";
            quantity = 0;
            coordinates = new HexCoordinates(0, 0);
            successfull = false;
            playername="";
            TribeId = 255;
            battleLog = new BattleLog();

        }
        public Feedback()
        {

        }
        
        public Feedback(int quantity, RessourceType ressource)
        {
            this.quantity = quantity;
            this.resource = ressource;
        }
        public Feedback(String msg)
        {
            message = msg;
        }

        public class BattleLog
        {
            public  Dictionary<TroopType, int> attackerTroops;
            public  Dictionary<TroopType, int> defenderTroops;
            public  Dictionary<TroopType, int> winnersRest;

            public enum BattleScore { attackerWon=0, defenderWon=2, Tie=1,buildingUndamaged=3}
            public BattleScore result;
             

        }

    }
}
