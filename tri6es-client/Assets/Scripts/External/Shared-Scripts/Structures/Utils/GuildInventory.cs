using System;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.Game;

namespace Shared.Structures
{
    public class GuildInventory
    {
        public readonly SortedDictionary<RessourceType, int> Resources = new SortedDictionary<RessourceType, int>()
        {
            // These are ordered by enum integer values (e.g. WOOD < COW), so appearance in UI may not be in this order.
            {RessourceType.COW, 0},
            {RessourceType.COAL, 0},
            {RessourceType.WOOD, 0},
            {RessourceType.STONE, 0},
            {RessourceType.WHEAT, 0},
        };
        
        public void DepositResources(Tribe tribe, Dictionary<RessourceType, int> resources)
        {
            if (tribe == null)
            {
                return;
            }
            
            foreach (KeyValuePair<RessourceType, int> pair in resources)
            {
                if (Resources.ContainsKey(pair.Key))
                {
                    Resources[pair.Key] += pair.Value;
                }
            }
            
            tribe.tribeInventory.ApplyRecipe(resources);
        }
        
        public void WithdrawResources(Tribe tribe, Dictionary<RessourceType, int> resources)
        {
            if (tribe == null)
            {
                return;
            }
            
            foreach (KeyValuePair<RessourceType, int> pair in resources)
            {
                if (Resources.ContainsKey(pair.Key))
                {
                    // Take away from guild
                    int currentValue = Resources[pair.Key];
                    Resources[pair.Key] = Math.Max(0, currentValue - pair.Value);
                    
                    // Add to tribe
                    tribe.tribeInventory.AddRessource(pair.Key, pair.Value);
                }
            }
        }

        public int GetResourceAmount(RessourceType type)
        {
            if (!Resources.ContainsKey(type))
            {
                return 0;
            }
            
            return Resources[type];
        }
    }
}