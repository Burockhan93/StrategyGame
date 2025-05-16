using System;using System.Collections.Generic;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Structures;

namespace Shared.Game
{
    public class Quest
    {
        public enum Kind {
            Ressource,
            Movement,
            Building,
        }

        public int Goal;

        public int Reward;
        public RessourceType RewardType;


        public RessourceType RessourceProgressType;
        public Type BuildingProgressType;

        public Kind kind;

        public int Progress;

        public bool isActive;

        public bool IsRessource => kind == Kind.Ressource;

        public bool isBuilding => kind == Kind.Building;

        public bool isMovement => kind == Kind.Movement;

        protected Quest(int goal, Kind kind, int reward, RessourceType rewardType, bool isActive, int progress)
        {
            Progress = progress;
            Goal = goal;
            this.kind = kind;
            Reward = reward;
            RewardType = rewardType;
            this.isActive = isActive;
        }

        /// <summary>Creates a new ressource collection quest.</summary>
        public static Quest CreateRessource(int goal, RessourceType ressourceProgressType, int reward, RessourceType rewardType, bool isActive, int progress = 0)
        {
            Quest quest = new Quest(goal, Kind.Ressource, reward, rewardType, isActive, progress);
            quest.RessourceProgressType = ressourceProgressType;
            return quest;
        }

        /// <summary>Creates a new building creation quest.</summary>
        public static Quest CreateBuilding(int goal, Type buildingProgressType, int reward, RessourceType rewardType, bool isActive, int progress = 0)
        {
            Quest quest = new Quest(goal, Kind.Building, reward, rewardType, isActive, progress);
            quest.BuildingProgressType = buildingProgressType;
            return quest;
        }

        /// <summary>Creates a new player movement quest.</summary>
        public static Quest CreateMovement(int goal, int reward, RessourceType rewardType, bool isActive, int progress = 0)
        {
            return new Quest(goal, Kind.Movement, reward, rewardType, isActive, progress);
        }

        public static List<List<Quest>> GetInitialQuestlines()
        {
            return new List<List<Quest>> {
                new List<Quest> {
                    Quest.CreateBuilding(1, typeof(Woodcutter), 5, RessourceType.WOOD, true),
                    Quest.CreateBuilding(1, typeof(Quarry), 5, RessourceType.STONE, false),
                    Quest.CreateBuilding(1, typeof(IronMine), 5, RessourceType.WOOD, false),
                    Quest.CreateBuilding(1, typeof(CoalMine), 5, RessourceType.COAL, false),
                    Quest.CreateBuilding(1, typeof(Smelter), 5, RessourceType.IRON, false),
                    Quest.CreateBuilding(1, typeof(CowFarm), 5, RessourceType.COW, false),
                    Quest.CreateBuilding(1, typeof(Tanner), 5, RessourceType.LEATHER, false),
                    Quest.CreateBuilding(1, typeof(WeaponSmith), 5, RessourceType.STONE, false),
                    Quest.CreateBuilding(1, typeof(ArmorSmith), 5, RessourceType.WOOD, false),
                    Quest.CreateBuilding(1, typeof(Barracks), 5, RessourceType.STONE, false),
                },
                new List<Quest> {
                    Quest.CreateMovement(100, 10, RessourceType.WOOD, true),
                    Quest.CreateMovement(500, 10, RessourceType.WOOD, false),
                    Quest.CreateMovement(1000, 10, RessourceType.WOOD, false),
                },
                new List<Quest> {
                    Quest.CreateRessource(100, RessourceType.WOOD, 10, RessourceType.WOOD, true),
                    Quest.CreateRessource(500, RessourceType.WOOD, 10, RessourceType.WOOD, false),
                    Quest.CreateRessource(1000, RessourceType.WOOD, 10, RessourceType.WOOD, false),
                },
                new List<Quest> {
                    Quest.CreateRessource(100, RessourceType.STONE, 10, RessourceType.STONE, true),
                    Quest.CreateRessource(500, RessourceType.STONE, 10, RessourceType.STONE, false),
                    Quest.CreateRessource(1000, RessourceType.STONE, 10, RessourceType.STONE, false),
                },
                new List<Quest> {
                    Quest.CreateRessource(100, RessourceType.COW, 10, RessourceType.COW, true),
                    Quest.CreateRessource(500, RessourceType.COW, 10, RessourceType.COW, false),
                    Quest.CreateRessource(1000, RessourceType.COW, 10, RessourceType.COW, false),
                },
                new List<Quest> {
                    Quest.CreateRessource(100, RessourceType.COAL, 10, RessourceType.COAL, true),
                    Quest.CreateRessource(500, RessourceType.COAL, 10, RessourceType.COAL, false),
                    Quest.CreateRessource(1000, RessourceType.COAL, 10, RessourceType.COAL, false),
                },
                new List<Quest> {
                    Quest.CreateRessource(100, RessourceType.WHEAT, 10, RessourceType.WHEAT, true),
                    Quest.CreateRessource(500, RessourceType.WHEAT, 10, RessourceType.WHEAT, false),
                    Quest.CreateRessource(1000, RessourceType.WHEAT, 10, RessourceType.WHEAT, false),
                },
            };
        }

        public bool addProgress(Player player, int amount)
        {   if(this.isActive && player.Tribe!=null){
            this.Progress += amount;
            if (this.Progress >= this.Goal) {
                System.Console.WriteLine("Finished Quest " + this.GetTask());
                this.isActive = false;
                player.Tribe.tribeInventory.AddRessource(this.RewardType, this.Reward);
                return true;
            }
        }
            return false;
        }
        public static List<uint> getAllQuests(Player player)
        {
            List<uint> activeQuests = new List<uint>();
            for (int i = 0; i < player.Quests.Count; i++)
            {
                bool foundactive = false;
                for (int j = 0; j < player.Quests[i].Count; j++)
                {
                    if (player.Quests[i][j].isActive)
                    {
                        foundactive = true;
                        activeQuests.Add(Convert.ToUInt32(i));
                        activeQuests.Add(Convert.ToUInt32(j));
                        activeQuests.Add(Convert.ToUInt32(player.Quests[i][j].Progress));
                    }else if(j == player.Quests[i].Count-1 && !foundactive){
                        activeQuests.Add(Convert.ToUInt32(i));
                        activeQuests.Add(Convert.ToUInt32(j+1));
                        activeQuests.Add(Convert.ToUInt32(player.Quests[i][j].Progress));
                    }
                }
            }
            return activeQuests;
        }
        public static List<List<uint>> addRessourceQuestProgress(Player player, HexCoordinates coords)
        {
           HexCell cell = GameLogic.grid.GetCell(coords);
           List<uint> successfulQuests = new List<uint>();
           List<List<uint>> combinedList = new List<List<uint>>();
           if (cell != null)
           {
               if (cell.Structure is Ressource)
               {
                    Ressource ressource = (Ressource)cell.Structure;
                    RessourceType ressourceType = ressource.ressourceType;
                    if (ressource.ManuallyHarvestable())
                    {
                        for (int i = 0; i < player.Quests.Count; i++)
                        {
                            for (int j = 0; j < player.Quests[i].Count; j++)
                            {
                                if (ressourceType == player.Quests[i][j].RessourceProgressType && player.Quests[i][j].kind == Kind.Ressource)
                                {
                                    if (player.Quests[i][j].addProgress(player, 1))
                                    {
                                        successfulQuests.Add(Convert.ToUInt32(i));
                                        successfulQuests.Add(Convert.ToUInt32(j));
                                        if (j + 1 < player.Quests[i].Count)
                                        {
                                            player.Quests[i][j+1].isActive = true;
                                        }
                                    }
                                }
                            }
                        }
                        List<uint> AllQuests = getAllQuests(player);
                        combinedList.Add(AllQuests);
                        combinedList.Add(successfulQuests);
                        return combinedList;
                   }
               }
           }
           return null;
        }
        public static List<List<uint>> addBuildingQuestProgress(Player player, Type buildingType)
        {
           List<uint> successfulQuests = new List<uint>();
           List<List<uint>> combinedList = new List<List<uint>>();
            for (int i = 0; i < player.Quests.Count; i++)
            {
                for (int j = 0; j < player.Quests[i].Count; j++)
                {
                    if (player.Quests[i][j].kind.Equals(Kind.Building) && player.Quests[i][j].BuildingProgressType.Equals(buildingType))
                    {
                        if (player.Quests[i][j].addProgress(player, 1))
                        {
                            successfulQuests.Add(Convert.ToUInt32(i));
                            successfulQuests.Add(Convert.ToUInt32(j));
                            if (j + 1 < player.Quests[i].Count)
                            {
                                player.Quests[i][j+1].isActive = true;
                            }
                        }
                    }
                }
            }
            List<uint> AllQuests = getAllQuests(player);
            combinedList.Add(AllQuests);
            combinedList.Add(successfulQuests);
            return combinedList;
        }
        public static List<List<uint>> addMovementQuestProgress(Player player, int distanceTravelled)
        {
           List<uint> successfulQuests = new List<uint>();
           List<List<uint>> combinedList = new List<List<uint>>();
            for (int i = 0; i < player.Quests.Count; i++)
            {
                for (int j = 0; j < player.Quests[i].Count; j++)
                {
                    if (player.Quests[i][j].kind == Kind.Movement)
                    {
                        if (distanceTravelled > 0 && distanceTravelled < 6)
                        {
                            if (player.Quests[i][j].addProgress(player, distanceTravelled))
                            {
                                successfulQuests.Add(Convert.ToUInt32(i));
                                successfulQuests.Add(Convert.ToUInt32(j));
                                if (j + 1 < player.Quests[i].Count)
                                {
                                    player.Quests[i][j+1].isActive = true;
                                }
                            }
                        }
                    }
                }
            }
            List<uint> AllQuests = getAllQuests(player);
            combinedList.Add(AllQuests);
            combinedList.Add(successfulQuests);
            return combinedList;
        }

        public string GetTask()
        {
            switch (kind)
            {
                case Kind.Ressource:
                    return $"Achievement Collect {Goal} {RessourceProgressType.ToFriendlyString()}";

                case Kind.Building:
                    return $"Build {Goal} {Structure.GetFriendlyName(BuildingProgressType)}";

                default:
                case Kind.Movement:
                    return $"Achievement Move {Goal} Cells";
            }
        }

        public string GetReward()
        {
            return $"{Reward}x {RewardType.ToFriendlyString()}";
        }
    }
}
