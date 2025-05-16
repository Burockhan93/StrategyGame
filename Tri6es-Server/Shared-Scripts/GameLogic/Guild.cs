using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;
using Shared.Structures;

namespace Shared.Game
{
    public class Guild
    {
        /// <summary>
        /// Needed to differentiate between existing and non-existing guilds.
        /// E.g. when sending a packet we usually send things like the guild's ID and its members.
        /// But when the guild's ID is ID_NO_GUILD we know that nothing else should be sent.
        /// </summary>
        public static readonly byte ID_NO_GUILD = 255;
        
        public byte Id;
        public Tribe FoundingMember;
        public List<Tribe> Members;

        public int Level;
        private readonly Dictionary<int, int> LevelToMaxMembersMap = new Dictionary<int, int>
        {
            {1, 3},
            {2, 4},
            {3, 5},
        };

        public GuildInventory Inventory;

        // SortedDictionary orders by key. Use OrderedDictionary if you want order by insertion.
        public readonly List<SortedDictionary<RessourceType, int>> ProgressMaps = new List<SortedDictionary<RessourceType, int>>
        {
            // Level 1 -> Level 2
            new SortedDictionary<RessourceType, int>
            {
                {RessourceType.COW, 20},
                {RessourceType.COAL, 20},
                {RessourceType.WOOD, 20},
                {RessourceType.STONE, 20},
                {RessourceType.WHEAT, 20},
            },
            // Level 2 -> Level 3
            new SortedDictionary<RessourceType, int>
            {
                {RessourceType.IRON, 20},
                {RessourceType.FOOD, 20},
                {RessourceType.LEATHER, 20},
            },
            // Level 3 upwards (empty map so that we don't have to deal with null)
            new SortedDictionary<RessourceType, int>(),
        };

        public Guild(byte id, Tribe founder)
        {
            Id = id;
            FoundingMember = founder;
            Members = new List<Tribe> {FoundingMember};
            
            Level = 1;
            Inventory = new GuildInventory();
        }

        public void LevelUp()
        {
            // Max level is 3
            Level = Math.Min(3, Level + 1);
        }

        public void AddDonations(Dictionary<RessourceType, int> donationMap)
        {
            SortedDictionary<RessourceType, int> progressMap = GetCurrentProgressMap();
            
            foreach (RessourceType key in donationMap.Keys)
            {
                int donationAmount = donationMap[key];
                int remainderAmount = progressMap[key];
                progressMap[key] = Math.Max(0, remainderAmount - donationAmount);
            }
        }

        public SortedDictionary<RessourceType, int> GetCurrentProgressMap()
        {
            return ProgressMaps[Level - 1];
        }

        public bool IsProgressReached()
        {
            if (Level == 3)
            {
                // Max level, no further progress.
                return false;
            }
            
            SortedDictionary<RessourceType, int> progressMap = GetCurrentProgressMap();
            return progressMap.Values.All(remainderCount => remainderCount <= 0);
        }

        public int GetMaxMembers()
        {
            return LevelToMaxMembersMap[Level];
        }

        public bool IsFull()
        {
            return Members.Count >= GetMaxMembers();
        }
    }
}