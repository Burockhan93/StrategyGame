using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Structures;
using Shared.DataTypes;
using Shared.Communication;

namespace Shared.Game
{
    public class Tribe
    {
        /// <summary> Needed to differentiate between existing and non-existing tribes. </summary>
        public static readonly byte ID_NO_TRIBE = 255;

        public byte Id;
        public Headquarter HQ;

        public Dictionary<Type, List<Building>> CurrentBuildings;

        public int size;

        public TribeInventory tribeInventory;

        public List<RessourceType> rtypes = new List<RessourceType>();

        public List<TroopType> ttypes = new List<TroopType>();

        public List<TribeBuff> Buffs = new List<TribeBuff>();

        public byte GuildId;

        public List<Research> Researches = new List<Research>();

        // Note: the order of the buildings in here also determines the order in which they're shown in the build menu.
        // Use alphabetical order.
        public static readonly Dictionary<Type, int>[] BuildingLimits = {
            // HQ level 1
            new Dictionary<Type, int>{
                { typeof(Bridge), 3 },
                { typeof(CowFarm), 2 },
                { typeof(Fisher), 2 },
                { typeof(Quarry), 2 },
                { typeof(LandRoad), 25 },   // Description is "Road"
                { typeof(Woodcutter), 2 },
                { typeof(WheatFarm), 2 },
            },
            // HQ level 2
            new Dictionary<Type, int>{
                { typeof(ArmorSmith), 4 },
                { typeof(AssemblyPoint), 5},
                { typeof(Bakery), 2 },
                { typeof(Barracks), 2 },
                { typeof(Bridge), 10 },
                { typeof(Butcher), 4 },
                { typeof(CoalMine), 2 },
                { typeof(CowFarm), 8 },
                { typeof(Fisher), 8 },
                { typeof(GuildHouse), 1 },
                { typeof(IronMine), 2 },
                { typeof(Library), 1 },
                { typeof(Market), 1 },
                { typeof(Quarry), 8 },
                { typeof(LandRoad), 40 },   // Description is "Road"
                { typeof(Smelter), 2 },
                { typeof(Storage), 3 },
                { typeof(Tanner), 8},
                { typeof(WheatFarm), 8 },
                { typeof(Woodcutter), 8 },
                { typeof(Workshop), 2 },
                { typeof(Tower), 10 },
                { typeof(WeaponSmith), 4 },
            },
            // HQ level 3
            new Dictionary<Type, int>{
                { typeof(ArmorSmith), 4 },
                { typeof(AssemblyPoint), 5},
                { typeof(Bakery), 2 },
                { typeof(Barracks), 2 },
                { typeof(Bridge), 10 },
                { typeof(Butcher), 4 },
                { typeof(CoalMine), 2 },
                { typeof(CowFarm), 8 },
                { typeof(Fisher), 8 },
                { typeof(GuildHouse), 1 },
                { typeof(IronMine), 2 },
                { typeof(Library), 1 },
                { typeof(Market), 1 },
                { typeof(Quarry), 8 },
                { typeof(LandRoad), 50 },   // Description is "Road"
                { typeof(Smelter), 2 },
                { typeof(Storage), 3 },
                { typeof(Tanner), 8},
                { typeof(WheatFarm), 8 },
                { typeof(Woodcutter), 8 },
                { typeof(Workshop), 2 },
                { typeof(Tower), 10 },
                { typeof(WeaponSmith), 4 },
            }
        };

        public List<Type> AvailableBuildings => BuildingLimits[HQ.Level - 1].Keys.ToList();

        public Tribe(byte id,Headquarter hq)
        {
            this.Id = id;
            this.HQ = hq;
            this.CurrentBuildings = new Dictionary<Type, List<Building>> {
                { typeof(Woodcutter), new List<Building>() },
                { typeof(LandRoad), new List<Building>() },
                { typeof(Storage), new List<Building>() },
                { typeof(Quarry), new List<Building>() },
                { typeof(CoalMine), new List<Building>() },
                { typeof(Smelter), new List<Building>() },
                { typeof(Bridge), new List<Building>() },
                { typeof(Market), new List<Building>() },
                { typeof(Fisher), new List<Building>() },
                { typeof(WheatFarm), new List<Building>() },
                { typeof(CowFarm), new List<Building>() },
                { typeof(Bakery), new List<Building>() },
                { typeof(Butcher), new List<Building>() },
                { typeof(Tanner), new List<Building>() },
                { typeof(Barracks), new List<Building>() },
                { typeof(Workshop), new List<Building>() },
                { typeof(Tower), new List<Building>() },
                { typeof(WeaponSmith), new List<Building>() },
                { typeof(ArmorSmith), new List<Building>() },
                { typeof(IronMine), new List<Building>() },
                { typeof(AssemblyPoint), new List<Building>() },
                { typeof(Library), new List<Building>() },
                { typeof(GuildHouse), new List<Building>() },
            };
            this.size = 1;
            this.tribeInventory = new TribeInventory(hq);
            this.GuildId = Guild.ID_NO_GUILD;
        }

        public Tribe(byte id,Headquarter hq,Dictionary<Type, List<Building>> currentBuildings)
        {
            this.Id = id;
            this.HQ = hq;
            this.CurrentBuildings = currentBuildings;
            this.size = 1;
            this.tribeInventory = new TribeInventory(hq);
            this.GuildId = Guild.ID_NO_GUILD;
        }

        public int GetBuildingLimit(Building building)
        {
            // get base limit
            int limit = BuildingLimits[HQ.Level - 1][building.GetType()];

            // increase by 10% of tribe size
            return (int) (limit * (1 + 0.1 * size));
        }

        public bool IsBuildingLimitReached(Building building)
        {
            return CurrentBuildings[building.GetType()].Count >= GetBuildingLimit(building);
        }

        public void AddBuilding(Building building)
        {
            Type buildingType = building.GetType();
            if(!CurrentBuildings.ContainsKey(buildingType))
            {
                CurrentBuildings.Add(buildingType, new List<Building>());
            }
            CurrentBuildings[buildingType].Add(building);
            if(building is Storage)
            {
                tribeInventory.AddStorageInventory(((Storage)building).Inventory);
            }
        }

        public void RemoveBuilding(Building building)
        {
            Type buildingType = building.GetType();
            CurrentBuildings[buildingType].Remove(building);
            if(building is Storage)
            {
                tribeInventory.RemoveStorageInventory(((Storage)building).Inventory);
            }
        }
        public void AddBuff(Collectable collectable)
        {
            if (collectable is RessourceBuff)
            {
                RessourceBuff r = (RessourceBuff)collectable;
                AddBuff(r);
            }
            if (collectable is TroopBuff)
            {
                TroopBuff t = (TroopBuff)collectable;
                AddBuff(t);
            }
        }
        public void AddBuff(RessourceBuff Buff)
        {
            if (rtypes.Contains(Buff.rtype))
            {
                foreach (TribeBuff buff in Buffs)
                {
                    if (buff.rtype == Buff.rtype)
                    {
                        buff.maxTime = buff.maxTime + Constants.HoursToGameTicks(1);
                        buff.ctime = buff.ctime + Constants.HoursToGameTicks(1);
                    }
                }
            }
            else
            {
                TribeBuff newBuff = new TribeBuff(Buff.rtype);
                Buffs.Add(newBuff);
                rtypes.Add(Buff.rtype);
            }



        }
        public void AddBuff(TroopBuff Buff)
        {
            if (ttypes.Contains(Buff.ttype))
            {
                foreach (TribeBuff buff in Buffs)
                {
                    if (buff.ttype == Buff.ttype)
                    {
                        buff.maxTime = buff.maxTime + Constants.HoursToGameTicks(1);
                        buff.ctime = buff.ctime + Constants.HoursToGameTicks(1);
                    }
                }
            }
            else
            {
                TribeBuff newBuff = new TribeBuff(Buff.ttype);
                Buffs.Add(newBuff);
                ttypes.Add(Buff.ttype);
            }



        }
        public List<TribeBuff> RemoveBuffs()
        {
            List<TribeBuff> inactives = new List<TribeBuff>();
            foreach (TribeBuff buff in Buffs)
            {
                if (buff.ctime == 0)
                {
                    inactives.Add(buff);
                }
            }
            return inactives;
        }
        public void RemoveBuff(TribeBuff Buff)
        {
            Buffs.Remove(Buff);
            rtypes.Remove(Buff.rtype);
            ttypes.Remove(Buff.ttype);
        }

        public bool HasGuild()
        {
            return GuildId != Guild.ID_NO_GUILD;
        }

        public bool HasNoGuild()
        {
            return GuildId == Guild.ID_NO_GUILD;
        }

        public bool HasGuildHouse()
        {
            return CurrentBuildings[typeof(GuildHouse)].Count > 0;
        }

        public GuildHouse GetGuildHouse()
        {
            if (HasGuildHouse())
            {
                List<Building> guildHouses = CurrentBuildings[typeof(GuildHouse)];
                if (guildHouses[0] is GuildHouse guildHouse)
                {
                    return guildHouse;
                }
            }
            
            return null;
        }

        public void AddResearch(Research research)
        {
            if (!Researches.Contains(research))
            {
                Researches.Add(research);
            }
        }

        public bool HasResearched(int researchCode)
        {
            // Check if any of the researches has the same code.
            return Researches.Any(research => research.Code == researchCode);
        }
    }
}
