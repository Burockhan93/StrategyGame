using System;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Structures;

namespace Shared.Game
{
    public class Player
    {
        public string Name;

        public Tribe Tribe;

        public HexCoordinates Position;

        public TroopInventory TroopInventory;

        public List<List<Quest>> Quests = new List<List<Quest>>();

        public byte Avatar;

        public Player(string name)
        {
            this.Name = name;
            this.Tribe = null;
            this.TroopInventory = new TroopInventory();
            Quests = Quest.GetInitialQuestlines();
            this.Avatar = (byte)255;
        }

        public Player(string name, Tribe tribe)
        {
            this.Name = name;
            this.Tribe = tribe;
            this.TroopInventory = new TroopInventory();
            Quests = Quest.GetInitialQuestlines();
            this.Avatar = (byte)255;
        }

        public Player(string name, Tribe tribe, TroopInventory TroopInventory, List<List<Quest>> quests)
        {
            this.Name = name;
            this.Tribe = tribe;
            this.TroopInventory = TroopInventory;
            Quests = quests;
            this.Avatar = (byte)255;
        }

        public bool hasActiveMovementQuest()
        {
            foreach (List<Quest> list in this.Quests)
            {
                foreach (Quest quest in list)
                {
                    if (quest.isActive && quest.isMovement)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public virtual void DoTick()
        {

        }
        public bool hasActiveRessourceQuest(HexCoordinates coordinates)
        {
            HexCell cell = GameLogic.grid.GetCell(coordinates);
            if (cell != null)
            {
                if (cell.Structure is Ressource)
                {
                    Ressource ressource = (Ressource)cell.Structure;
                    RessourceType ressourceType = ressource.ressourceType;
                    foreach (List<Quest> list in this.Quests)
                    {
                        foreach (Quest quest in list)
                        {
                            if (quest.isActive && quest.IsRessource && ressourceType == quest.RessourceProgressType)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool hasActiveBuildingQuest(Type buildingType)
        {
            foreach (List<Quest> list in this.Quests)
            {
                foreach (Quest quest in list)
                {
                    if (quest.isActive && quest.isBuilding && (quest.BuildingProgressType.Equals(buildingType)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
