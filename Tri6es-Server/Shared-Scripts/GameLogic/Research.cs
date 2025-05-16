using System.Collections.Generic;
using Shared.DataTypes;

namespace Shared.Game
{
    public class Research
    {
        // Codes
        public const int AREA_RECONNAISSANCE = 0;
        public const int CARRIER_PIGEON = 1;

        public int Code;
        public string Name;
        public string Description;
        public Dictionary<RessourceType, int> Costs;

        public Research(int researchCode)
        {
            Costs = new Dictionary<RessourceType, int>();
            
            switch (researchCode)
            {
                case AREA_RECONNAISSANCE:
                    Code = AREA_RECONNAISSANCE;
                    Name = "Area Reconnaissance";
                    Description = "Increases the field of view around the player, showing more of the surroundings.";
                    Costs.Add(RessourceType.COW, 5);
                    Costs.Add(RessourceType.FOOD, 2);
                    break;
                case CARRIER_PIGEON:
                    Code = CARRIER_PIGEON;
                    Name = "Carrier Pigeon";
                    Description = "Allows guild members to see scouting results of other guild members.";
                    Costs.Add(RessourceType.IRON, 20);
                    Costs.Add(RessourceType.LEATHER, 20);
                    break;
                // Add other research options here.
            }
        }
    }
}