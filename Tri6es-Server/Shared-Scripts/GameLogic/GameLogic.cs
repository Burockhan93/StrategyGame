using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shared.Communication;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Structures;
using System.Net;
using System.Xml;
using System.Globalization;
using System.IO;

namespace Shared.Game
{
    public static class GameLogic
    {
        public static DateTime time = DateTime.Now;
        /// <summary>Cooldown for assembly point attacks.</summary>
        public static readonly int AP_ATTACK_COOLDOWN = Constants.MinutesToGameTicks(3);
        
        private static int Progress = 0;

        public static bool initialized { get; private set; } = false;

        public static HexGrid.HexGrid grid { get; private set; }

        private static List<Building> buildings = new List<Building>();

        private static List<Ressource> ressources = new List<Ressource>();
        public static IReadOnlyCollection<Ressource> Res { get { return ressources.AsReadOnly(); } }

        private static List<Collectable> collectables = new List<Collectable>();

        private static List<Cart> carts = new List<Cart>();

        public static List<Tribe> Tribes = new();

        public static List<Player> Players = new();
        
        public static List<Guild> Guilds = new List<Guild>();

        private static Random random = new Random();

        private static String _currentTime;

        public static void newGameInit(HexGrid.HexGrid hexGrid)
        {
            initialized = true;
            grid = hexGrid;
            InitializeAI();
            foreach (HexCell cell in grid.cells)
            {
                if (cell.Structure != null)
                {
                    AddStructureToList(cell.Structure);
                }
            }
            ComputeConnectedStorages();

            foreach (Building building in buildings)
            {
                Tribe tr = GetTribe(building.Tribe);
                tr.AddBuilding(building);
            }


        }

        private static void InitializeAI()
        {
            Headquarter AIHeqadquarter = new Headquarter();
            Tribe tribe = ApplyBuildHQ(new HexCoordinates(5, 5), AIHeqadquarter);
            //Tribe tribe = ApplyBuildHQ(new HexCoordinates(2,32), AIHeqadquarter);
            AIPlayer AIPlayer;
            for (int i = 0; i < 2; i++)
            {
                AIPlayer = new AIPlayer($"AI{i}", tribe);
                AddPlayer(AIPlayer.Name, AIPlayer.Tribe.Id, new HexCoordinates(i, i), AIPlayer.TroopInventory, AIPlayer.Quests, 1);
            }
        }

        public static void Init(HexGrid.HexGrid hexGrid)
        {
            initialized = true;
            grid = hexGrid;
            Console.WriteLine(grid.cells.Length);
            //buildings = new List<Building>();

            //ressources = new List<Ressource>();

            //collectables = new List<Collectable>();

            //carts = new List<Cart>();

            foreach (HexCell cell in grid.cells)
            {
                if (cell.Structure != null)
                {
                    AddStructureToList(cell.Structure);
                }
            }
            ComputeConnectedStorages();

            foreach (Building building in buildings)
            {
                if (building is Headquarter )
                    AddTribe(building.Tribe, (Headquarter)building);
            }

            foreach (Building building in buildings)
            {
                Tribe tribe = GetTribe(building.Tribe);
                tribe.AddBuilding(building);
            }
        }

        #region PLAYERS

        public static Player AddPlayer(string name, Tribe tribe)
        {
            Player newPlayer;
            if (name.StartsWith("AI"))
            {
                newPlayer = new AIPlayer(name, tribe);
            }
            else
            {
                newPlayer = new Player(name, tribe);
            }
           
            Players.Add(newPlayer);
            return newPlayer;
        }

        public static void AddPlayer(string playerName, int tribeId, HexCoordinates coordinates, TroopInventory troopInventory, List<List<Quest>> quests, byte avatar)
        {
            Tribe tribe = GameLogic.GetTribe(tribeId);
            Player player;
            if (playerName.StartsWith("AI"))
            {
                player = (AIPlayer)GameLogic.GetPlayer(playerName);
            }
            else
            {
                player = GameLogic.GetPlayer(playerName);
            }
            
            if (player == null)
                player = AddPlayer(playerName, tribe);

            player.Tribe = tribe;
            player.Position = coordinates;
            player.TroopInventory = troopInventory;
            player.Quests = quests;
            player.Avatar = avatar;
        }

        public static Player GetPlayer(string name)
        {
            foreach (Player player in Players)
            {
                if (player.Name == name)
                    return player;
            }
            return null;
        }

        public static Tribe GetTribe(int id)
        {
            foreach (Tribe tribe in Tribes)
            {
                if (tribe.Id == id)
                    return tribe;
            }
            return null;
        }

        public static Tribe AddTribe(byte tribeID, Headquarter hq)
        {
            Tribe newTribe = new Tribe(tribeID, hq);
            Tribes.Add(newTribe);
            return newTribe;
        }

        public static bool RemoveTribe(byte tribeID)
        {
            return Tribes.Remove(GetTribe(tribeID));
        }

        public static List<Building> getAssemblyPointsInRange(Tribe tribe, Building building)
        {
            Type apt = new AssemblyPoint().GetType();
            if (!tribe.CurrentBuildings.ContainsKey(apt))
            {
                return new List<Building>();
            }
            List<Building> aps = tribe.CurrentBuildings[apt];
            List<Building> result = new List<Building>();
            foreach (Building ap in aps)
            {
                if (((AssemblyPoint)ap).Cooldown > 0)
                {
                    continue;
                }
                int res = Pathfinding.getShortestPathAStar(grid, building.Cell, ap.Cell, tribe);
                if (res >= 0)
                {
                    result.Add((ProtectedBuilding)ap);
                }
            }
            return result;
        }

        public static List<Building> getAssemblyPointsInRangeWithoutCooldown(Tribe tribe, Building building)
        {
            Type apt = new AssemblyPoint().GetType();
            if (!tribe.CurrentBuildings.ContainsKey(apt))
            {
                return new List<Building>();
            }
            List<Building> aps = tribe.CurrentBuildings[apt];
            List<Building> result = new List<Building>();
            foreach (Building ap in aps)
            {
                int res = Pathfinding.getShortestPathAStar(grid, building.Cell, ap.Cell, tribe);
                if (res >= 0)
                {
                    result.Add((ProtectedBuilding)ap);
                }
            }
            return result;
        }

        #endregion
        
        #region GUILD
        
        public static Guild GetGuild(int guildId)
        {
            return Guilds.Find(guild => guild.Id == guildId);
        }

        public static Guild AddGuild(byte guildId, Tribe foundingTribe)
        {
            Guild guild = new Guild(guildId, foundingTribe);
            Guilds.Add(guild);
            return guild;
        }

        public static void DonateToGuild(Guild guild, Tribe tribe, Dictionary<RessourceType, int> donation)
        {
            // Move donated resources from tribe's inventory to guild
            guild.AddDonations(donation);
            tribe.tribeInventory.ApplyRecipe(donation);
        }
        
        #endregion

        public static int Harvest(string playerName, HexCoordinates coords, ref Feedback feedback)
        {
            HexCell cell = grid.GetCell(coords);
            Player player = GetPlayer(playerName);
            GuildHouse guildHouse = null;
            Tribe tribe = player.Tribe;
            if (tribe != null)
            {
                guildHouse = tribe.GetGuildHouse();  // May be null, which is OK. GuildHouse should be built for boost to have effect.
            }

            //Cheats
            if (coords.X == 0 && coords.Y == 0)
            {
                foreach (RessourceType resource in Enum.GetValues(typeof(RessourceType)))
                {
                    tribe.tribeInventory.AddRessource(resource, 200);

                    Console.WriteLine($"Added {resource}");
                }
                return 99;
            }
            if (cell != null)
            {
                if (cell.Structure is Ressource)
                {
                    Ressource ressource = (Ressource)cell.Structure;
                    RessourceType ressourceType = ressource.ressourceType;
                    feedback.resource = ressourceType;
                    if (ressource.ManuallyHarvestable())
                    {
                        int boost = 0;
                        if (guildHouse != null && 
                            // resourceType is always != null
                            (ressourceType == guildHouse.BoostBaseResource 
                            || ressourceType == guildHouse.BoostAdvancedResource
                            || ressourceType == guildHouse.BoostWeapon))
                        {
                            boost = 1;
                        }
                        if (tribe.rtypes.Contains(ressourceType))
                        {
                            boost++;
                        }
                        int gain = ressource.HarvestManuallyWithBoost(boost);
                        Console.WriteLine($"Gain == {gain}, boost == {boost}");
                        tribe.tribeInventory.AddRessource(ressourceType, gain);

                        feedback.quantity = gain;
                        feedback.successfull = true;
                        //cell.Structure = new Empty(cell);
                        DestroyStructure(cell.coordinates);
                        return gain;
                    }
                }
            }
            feedback.quantity = 0;
            feedback.successfull = false;
            return 0;
        }

        public static bool Collect(string playername, HexCoordinates coords, ref Feedback feedback) 
        {
            HexCell cell = grid.GetCell(coords);
            Player player = GetPlayer(playername);
            Tribe tribe = player.Tribe;

            if(cell != null) 
            {
                if (cell.Structure is Collectable) 
                {
                    Collectable collectable = (Collectable)cell.Structure;
                    CollectableType collectableType = collectable.type;

                    if (collectable is Loot)
                    {
                        int gain = collectable.Collect(tribe, player);

                    }
                    if (collectable is RessourceBuff)
                    {
                        int time = collectable.Collect(tribe, player);

                    }
                    if (collectable is TroopBuff)
                    {
                        int time = collectable.Collect(tribe, player);
                    }

                }
                DestroyStructure(cell.coordinates);
                return true;
            }

            return false;
        }

        public static void ApplyResearch(Tribe tribe, Research research)
        {
            tribe.tribeInventory.ApplyRecipe(research.Costs);
            tribe.AddResearch(research);
        }

        #region BUILDINGS

        public static bool PlayerInRange(HexCoordinates coords, Player player)
        {
            if (coords == player.Position)
            {
                return true;
            }
            for (HexDirection dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
            {
                if (coords != player.Position.InDirection(dir))
                    continue;
                return true;
            }
            return false;
        }

        public static bool VerifyBuild(HexCoordinates coords, Type buildingType, Player player, ref Feedback feedback)
        {
            Building building = (Building)Activator.CreateInstance(buildingType);
            if (building == null)
            {
                feedback.message = $"Could not instantiate a building typeof {buildingType} please refer to VerifyBuildMethod in GameLogic";
                return false;
            }

            if (player.Tribe == null)
            {
                feedback.message = $"Building could not be build because player doesnt have a tribe";
                return false;
            }

            //check if the player is adjacent to the position where the building is supposed to placed
            if (!PlayerInRange(coords, player))
            {
                feedback.message = "Too far away from the construction site";
                return false;
            }

            HexCell cell = grid.GetCell(coords);
            building.Tribe = player.Tribe.Id;

            //check if the building can be placed at the position
            if (!building.IsPlaceableAt(cell))
            {
                feedback.message = "This position has been claimed by another tribe";
                return false;
            }

            if (player.Tribe.IsBuildingLimitReached(building))
            {
                feedback.message = $"The limit for {building.GetFriendlyName()} has been reached.";
                return false;
            }


            if (!player.Tribe.tribeInventory.RecipeApplicable(building.Recipes[0],ref feedback))
            {
                feedback.message += "to build";
                return false;
            }
            
            //Successfully built
            feedback.message = "";
            feedback.successfull = true;
            return true;
        }

        /// <summary>Places a building structure on the given cell.</summary>
        public static Building PlaceBuilding(HexCell cell, Type buildingType, Tribe tribe)
        {
            // create new building
            Building building = (Building)Activator.CreateInstance(buildingType);
            building.Tribe = tribe.Id;
            if (cell.Structure is Ressource)
            {
                building.CellRessource = ((Ressource)cell.Structure);
            }
            
            // destroy existing structure
            if (cell.Structure != null  && !(cell.Structure is Building))
            {
                DestroyStructure(cell.coordinates);
            }
            
            // update cell
            cell.Structure = building;
            building.Cell = cell;

            return building;
        }

        /// <summary>Creates a new building and applies the recipe to the tribe inventory.</summary>
        public static HexCell ApplyBuild(HexCoordinates coords, Type buildingType, Tribe tribe)
        {
            HexCell cell = grid.GetCell(coords);
            Building building = PlaceBuilding(cell, buildingType, tribe);

            tribe.tribeInventory.ApplyRecipe(building.Recipes[0]);

            AddStructureToList(building);

            Headquarter hq = tribe.HQ;
            HexCell hc = hq.Cell;
            int distance = Pathfinding.getShortestPathAStar(grid, cell, hc, tribe);

            tribe.AddBuilding(building);

            if (building is ICartHandler)
            {
                ComputeConnectedStorages();
            }

            if (building is ProtectedBuilding)
            {
                ProtectedBuilding protectedbuilding = (ProtectedBuilding)building;
                protectedbuilding.ProtectRadius();
            }
            
            if (tribe.HasGuild() && building is GuildHouse)
            {
                Guild guild = GetGuild(tribe.GuildId);
                ChangeGuildHouseLevel(tribe, (byte) guild.Level);
            }
            
            return cell;
        }

        public static bool VerifyRepair(HexCoordinates coords, Player player)
        {
            HexCell cell = grid.GetCell(coords);

            if (cell == null)
            {
                return false;
            }

            if (player.Tribe == null)
            {
                return false;
            }
            if (cell.GetCurrentTribe() != player.Tribe.Id)
            {
                return false;
            }

            if (!PlayerInRange(coords, player))
            {
                return false;
            }

            if (cell.Structure is Ruin)
            {
                Ruin ruin = (Ruin)cell.Structure;
                if (ruin.Tribe != player.Tribe.Id)
                {
                    return false;
                }
                if (player.Tribe.tribeInventory.RecipeApplicable(ruin.Recipes[ruin.Level]))
                    return true;
            }
            return false;
        }
        public static bool verifySalvage(HexCoordinates coords, Player player)
        {
            HexCell cell = grid.GetCell(coords);

            if (cell == null)
            {
                return false;
            }

            if (player.Tribe == null)
            {
                return false;
            }
            if (cell.GetCurrentTribe() != player.Tribe.Id)
            {
                return false;
            }

            if (!PlayerInRange(coords, player))
            {
                return false;
            }

            if (cell.Structure is Ruin)
            {
                Ruin ruin = (Ruin)cell.Structure;
                if (ruin.Tribe != player.Tribe.Id)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>Repairs a ruin structure.</summary>
        public static void ApplyRepairBuilding(HexCoordinates coords, Tribe tribe)
        {
            HexCell cell = grid.GetCell(coords);

            if (cell.Structure is Ruin)
            {
                // place the repaired building
                Type buildingType = ((Ruin)cell.Structure).BuildingType;
                byte level = ((Ruin)cell.Structure).Level;
                if (cell.Structure != null)
                {
                    DestroyStructure(cell.coordinates);
                }

                // create new building
                Building building = (Building)Activator.CreateInstance(buildingType);
                building.Tribe = tribe.Id;
                building.Level = level;

                // update cell
                cell.Structure = building;
                building.Cell = cell;
                Dictionary<RessourceType, int> newrecipe = new Dictionary<RessourceType, int>();
                foreach (RessourceType rs in building.Recipes[level - 1].Keys)
                {
                    newrecipe.Add(rs, (int)(building.Recipes[level - 1][rs] * 0.5));
                }

                tribe.tribeInventory.ApplyRecipe(newrecipe); //TODO: apply the appropriate recipe here (with level and probably a dict in Ruin)

                AddStructureToList(building);

                tribe.AddBuilding(building);

                if (building is ICartHandler)
                {
                    ComputeConnectedStorages();
                }

                if (building is ProtectedBuilding)
                {
                    ProtectedBuilding protectedbuilding = (ProtectedBuilding)building;
                    protectedbuilding.ProtectRadius();
                }
            }
        }
        public static void ApplySalvage(HexCoordinates coordinates, Tribe tribe)
        {
            HexCell cell = grid.GetCell(coordinates);


            if (cell.Structure is Ruin)
            {

                Ruin r = (Ruin)cell.Structure;
                byte level = r.Level;
                Dictionary<RessourceType, int> recipe = r.Recipes[level - 1];
                foreach (RessourceType type in recipe.Keys)
                {
                    int amount = ((int)(recipe[type] * 0.5));
                    tribe.tribeInventory.AddRessource(type, recipe[type]);
                }
                DestroyStructure(coordinates);
            }
        }

        public static bool VerifyUpgrade(HexCoordinates coords, Player player, ref Feedback feedback)
        {
            HexCell cell = grid.GetCell(coords);
            feedback.type = cell.Structure.GetType();


            if (cell == null)
            {
                feedback.message = "Null Cell";
                return false;
            }

            if (player.Tribe == null)
            {
                feedback.message = "Join a tribe!";
                return false;
            }

            if (!PlayerInRange(coords, player))
            {
                feedback.message = "Get in Range of the building";
                return false;
            }

            if (cell.Structure is Building)
            {
                Building building = (Building)cell.Structure;
                
                if (building.Tribe != player.Tribe.Id)
                {
                    feedback.message = "This building doesn't belong to your tribe";
                    return false;
                }

                if (!building.IsUpgradable())
                {
                    feedback.message = "This building is not upgradable";
                    return false;
                }
                if (player.Tribe.tribeInventory.RecipeApplicable(building.Recipes[building.Level],ref feedback)) 
                {
                    feedback.message = "";
                    feedback.successfull = true;
                    return true;
                }
                    
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static void ApplyUpgrade(HexCoordinates coords, Tribe tribe)
        {
            HexCell cell = grid.GetCell(coords);
            if (cell.Structure is Building)
            {
                Building building = (Building)cell.Structure;
                if (!building.IsUpgradable())
                    return;

                tribe.tribeInventory.ApplyRecipe(building.Recipes[building.Level]);
                ((Building)cell.Structure).Upgrade();
            }
        }

        private static void ApplyDowngrade(HexCoordinates coords)
        {
            HexCell cell = grid.GetCell(coords);
            Structure structure = cell.Structure;
            if (structure != null && structure is ProtectedBuilding)
            {
                ProtectedBuilding protector = (ProtectedBuilding)structure;
                int radius = protector.Radius;

                // remove protector from cells
                foreach (HexCell protectedCell in protector.GetProtectedCells())
                {
                    protectedCell.Protectors.Remove(protector.Cell.coordinates);
                }

                // turn no longer protected buildings into ruins
                foreach (Building building in cell.GetNeighborStructures<Building>(radius))
                {
                    if (building != protector && !building.Cell.isProtected())
                    {
                        if (building.Level > 1)
                            building.Downgrade();
                        DestroyStructure(building.Cell.coordinates);
                        building.Cell.Structure = new Ruin(building.Cell, building.Tribe, building.Level, building.GetType());
                    }
                }
            }
        }

        private static void AddStructureToList(Structure structure)
        {
            if (typeof(Building).IsAssignableFrom(structure.GetType()))
            {
                if (!buildings.Contains(structure))
                {
                    buildings.Add((Building)structure);
                }
                if (structure is ICartHandler)
                {
                    foreach (Cart cart in ((ICartHandler)structure).Carts)
                    {
                        if (!carts.Contains(cart))
                        {
                            carts.Add(cart);
                        }
                    }
                }
            }
            else if (typeof(Ressource).IsAssignableFrom(structure.GetType()))
            {
                if (!ressources.Contains(structure))
                {
                    ressources.Add((Ressource)structure);
                }
            } 
            else if(typeof(Collectable).IsAssignableFrom(structure.GetType()))
            {
                if (!collectables.Contains(structure)) 
                {
                    collectables.Add((Collectable)structure);
                }
            }
        }
        public static bool DestroyStructure(HexCoordinates coords, ref Feedback feedback)
        {
            
            HexCell cell = grid.GetCell(coords);

            if (cell == null)
            {
                feedback.successfull = false;
                return false;
            }

            Structure structure = cell.Structure;
            Console.WriteLine(structure + " has been destroyed");
            if (structure != null)
            {
                if (structure is ProtectedBuilding)
                {
                    ApplyDowngrade(coords);
                }
                cell.Structure = null;
                if (typeof(Building).IsAssignableFrom(structure.GetType()))
                {
                    feedback.type = structure.GetType();

                    Building building = (Building)structure;

                    //Remove the building from logic. Only one building will be removed. nevertheles until i am sure of it RemoveAll function stays
                    buildings.RemoveAll(elem => elem == building);
                    // remove the carriages whose origin is destroyed building.Only one carriage will be removed. nevertheles until i am sure of it RemoveAll function stays
                    feedback.quantity = carts.RemoveAll(elem => elem.Origin == building);

                    foreach (Building b in buildings)
                    {
                        if (b is ICartHandler)
                        {
                           
                             ((ICartHandler)b).Carts.RemoveAll(elem => elem.Origin == building);

                            if (b is InventoryBuilding)
                            {
                                if (building is InventoryBuilding)
                                {
                                    ((InventoryBuilding)b).AllowedRessources.Remove((InventoryBuilding)building);
                                    System.Console.WriteLine("b: " + b + " building: " + building + " removed");
                                }
                            }
                        }
                    }
                    if (structure is ICartHandler)
                    {
                        ComputeConnectedStorages();
                    }
                    Tribe tribe = GetTribe(building.Tribe);

                    if (!(building is Ruin)) tribe.RemoveBuilding(building);
                    if (building.CellRessource != null)
                    {
                        cell.Structure = ((Structure)building.CellRessource);
                    }
                }
                else if (typeof(Ressource).IsAssignableFrom(structure.GetType()))
                {
                    ressources.RemoveAll(elem => elem == structure);
                }
            }

            feedback.successfull = true;
            return true;
        }
        public static bool DestroyStructure(HexCoordinates coords)
        {
            HexCell cell = grid.GetCell(coords);

            if (cell == null)
            {
                return false;
            }
               
            Structure structure = cell.Structure;
            Console.WriteLine(structure + " has been destroyed");
            if (structure != null)
            {
                if (structure is ProtectedBuilding)
                {
                    ApplyDowngrade(coords);
                }
                
                cell.Structure = null;
                if (typeof(Building).IsAssignableFrom(structure.GetType()))
                {
                    Building building = (Building)structure;
                    
                    //TEST
                    buildings.Remove(building);
                   // buildings.RemoveAll(elem => elem==building);
                    carts.RemoveAll(elem => elem.Origin == building);
                    foreach (Building b in buildings)
                    {
                        if (b is ICartHandler)
                        {
                            ((ICartHandler)b).Carts.RemoveAll(elem => elem.Origin == building);
                            if (b is InventoryBuilding)
                            {
                                //if (building is InventoryBuilding)
                                //If this building belongs to our own Tribe
                                if (building is InventoryBuilding && building.Tribe ==b.Tribe)
                                {
                                    ((InventoryBuilding)b).AllowedRessources.Remove((InventoryBuilding)building);
                                    System.Console.WriteLine("b: " + b + " building: " + building + " removed");
                                }
                            }
                        }
                    }
                    if (structure is ICartHandler)
                    {
                        ComputeConnectedStorages();
                    }
                    Tribe tribe = GetTribe(building.Tribe);

                    if (!(building is Ruin) && !(building is Headquarter)) tribe.RemoveBuilding(building);
                    if (building.CellRessource != null)
                    {
                        cell.Structure = ((Structure)building.CellRessource);
                    }

                    if (building is Headquarter)
                    {
                        //Remove tribe from the players
                        foreach (Player pl in GameLogic.Players)
                        {
                            if (pl.Tribe?.Id == building.Tribe) pl.Tribe = null;
                        }
                        //RemoveTribe form GameLogic
                        RemoveTribe(building.Tribe);

                    }

                }
                else if (typeof(Ressource).IsAssignableFrom(structure.GetType()))
                {
                    ressources.RemoveAll(elem => elem == structure);
                }
            }

            return true;
        }

        public static bool VerifyBuildHQ(HexCoordinates coords, Headquarter hq, Player player)
        {
            HexCell cell = grid.GetCell(coords);

            if (hq == null)
            {
                return false;
            }

            if (!PlayerInRange(coords, player))
            {
                return false;
            }

            hq.Tribe = (byte)Tribes.Count;

            //check if the building can be placed at the position
            if (!hq.IsPlaceableAt(cell))
            {
                return false;
            }

            if (player.Tribe != null)
            {
                return false;
            }

            return true;
        }

        public static Tribe ApplyBuildHQ(HexCoordinates coords, Headquarter hq)
        {
            HexCell cell = grid.GetCell(coords);
            if (cell.Structure != null)
            {
                DestroyStructure(coords);
            }
            cell.Structure = hq;
            hq.Cell = cell;

            Tribe tribe = AddTribe((byte)Tribes.Count, hq);
            hq.Tribe = tribe.Id;

            hq.ProtectRadius();

            AddStructureToList(hq);

            ComputeConnectedStorages();

            return tribe;
        }
        
        public static void CreateGuild(Tribe foundingTribe)
        {
            List<byte> usedGuildIds = new List<byte>();
            usedGuildIds.AddRange(Guilds.Select(g => g.Id));

            byte newGuildId = 0;
            while (usedGuildIds.Contains(newGuildId) && newGuildId != Guild.ID_NO_GUILD)
            {
                newGuildId++;
            }
            
            Guild guild = AddGuild(newGuildId, foundingTribe);
            foundingTribe.GuildId = guild.Id;
        }

        public static void LeaveGuild(Tribe leavingTribe)
        {
            Guild guild = GetGuild(leavingTribe.GuildId);
            if (guild != null)
            {
                guild.Members.Remove(leavingTribe);
                leavingTribe.GuildId = Guild.ID_NO_GUILD;
                
                if (guild.Members.Count == 0)
                { 
                    // No tribes left in the guild -> disband (what about guild ID?)
                    Guilds.Remove(guild);
                }
                else if (leavingTribe == guild.FoundingMember)
                {
                    // Founding tribe (== master) left -> need new master
                    // For now just take the next member in the list
                    guild.FoundingMember = guild.Members[0];
                }
            }
        }
        
        public static void JoinGuild(byte guildId, Tribe joiningTribe)
        {
            Guild guild = GetGuild(guildId);
            if (guild != null)
            {
                // TODO: make sure same tribe is not added twice, respect max size of guild, etc.
                guild.Members.Add(joiningTribe);
                joiningTribe.GuildId = guild.Id;
            }
        }

        public static void ChangeGuildHouseLevel(Tribe tribe, byte newLevel)
        {
            GuildHouse guildHouse = tribe.GetGuildHouse();
            if (guildHouse != null)
            {
                guildHouse.Level = newLevel;
            }
        }

        public static bool MoveTroops(HexCoordinates sourceCoords, HexCoordinates destCoords, TroopType troopType, int amount)
        {
            ProtectedBuilding source = (ProtectedBuilding)grid.GetCell(sourceCoords).Structure;
            ProtectedBuilding dest = (ProtectedBuilding)grid.GetCell(destCoords).Structure;
            if (amount > 0)
            {
                return dest.TroopInventory.MoveTroops(source.TroopInventory, troopType, Mathf.Abs(amount));
            }
            else
            {
                return source.TroopInventory.MoveTroops(dest.TroopInventory, troopType, Mathf.Abs(amount));
            }
        }

        public static void Fight(Player player, HexCoordinates attackerCoords, HexCoordinates defenderCoords, ref Feedback feedback)
        {
            // get attacking assembly point
            Structure attackerStructure = grid.GetCell(attackerCoords).Structure;
            if (attackerStructure is AssemblyPoint)
            {
                AssemblyPoint attacker = (AssemblyPoint)attackerStructure;

                // check assembly point cooldown
                if (attacker.Cooldown > 0)
                {
                    feedback.message = "Assembly Point needs to cooldown first!";
                    return;
                }

                // get target building
                Structure defenderStructure = grid.GetCell(defenderCoords).Structure;
                
                if (defenderStructure is ProtectedBuilding)
                {
                    ProtectedBuilding defender = (ProtectedBuilding)defenderStructure;

                    // check for protected hq
                    if (defender is Headquarter && ((Headquarter)defender).isProtected())
                    {
                        feedback.message = "This HQ is being protected";
                        return;
                    }

                    // execute fight
                    feedback.successfull = true;
                    feedback.type = defenderStructure.GetType();
                    // defenders' buildings infos
                    Building building = (Building)defenderStructure;
                    feedback.TribeId = building.Tribe;
                    feedback.coordinates = defenderCoords;
                    
                    TroopInventory attackerTroops = attacker.TroopInventory;
                    byte levels = attackerTroops.AttackBuilding(defender, player.Tribe, ref feedback);

                    if (feedback.battleLog.result == Feedback.BattleLog.BattleScore.defenderWon) feedback.message = "Battle is lost!";
                    
                    //Fight is over and building could be damaged.
                    if (levels >= defender.Level && levels !=0)
                    {
                        DestroyStructure(defender.Cell.coordinates);
                       
                        feedback.message = "Battle is won! You destroyed the building";                    
                    }
                    else if((levels < defender.Level && levels != 0))
                    {
                        int counter = 0;
                        do
                        {
                            counter++;
                            ApplyDowngrade(defender.Cell.coordinates);
                            levels--;
                            defender.Downgrade();
                        }
                        while (levels > 0);

                        feedback.message = $"Battle is won! Couldn't destroy but downgraded the building by {counter}";

                    }

                    // apply cooldowns
                    int distance = Pathfinding.getShortestPathAStar(grid, grid.GetCell(defenderCoords), grid.GetCell(attackerCoords), player.Tribe);
                    //TEST
                    //attacker.Cooldown = (int)distance * AP_ATTACK_COOLDOWN;
                    attacker.Cooldown = 0;
                    
                    return;

                }
                feedback.message = "Only ProtectBuildings can be attacked!";
            }

        }

        public static bool ChangeAllowedRessource(HexCoordinates origin, HexCoordinates destination, RessourceType ressourceType, bool newValue)
        {
            HexCell originCell = grid.GetCell(origin);
            HexCell destinationCell = grid.GetCell(destination);

            if (originCell.Structure is InventoryBuilding && destinationCell.Structure is InventoryBuilding)
            {
                InventoryBuilding originBuilding = (InventoryBuilding)originCell.Structure;
                InventoryBuilding destinationBuilding = (InventoryBuilding)destinationCell.Structure;

                originBuilding.AllowedRessources[destinationBuilding][ressourceType] = newValue;
                return true;
            }
            return false;
        }

        public static bool ChangeTroopRecipeOfBarracks(HexCoordinates barracksCoordinates, TroopType troopType)
        {
            HexCell cell = grid.GetCell(barracksCoordinates);
            if (cell != null)
            {
                if (cell.Structure is Barracks)
                {
                    ((Barracks)cell.Structure).ChangeTroopRecipe(troopType);
                    return true;
                }
            }
            return false;
        }

        public static bool ChangeStrategyOfProtectedBuilding(HexCoordinates coordinates, int oldIndex, int newIndex)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null)
            {
                if (cell.Structure is ProtectedBuilding)
                {
                    ((ProtectedBuilding)cell.Structure).TroopInventory.UpdateStrategy(oldIndex, newIndex);
                    return true;
                }
            }
            return false;
        }

        public static bool ChangeStrategyOfPlayer(string playerName, int oldIndex, int newIndex)
        {
            Player player = GetPlayer(playerName);
            if (player != null)
            {
                player.TroopInventory.UpdateStrategy(oldIndex, newIndex);
                return true;
            }
            return false;
        }

        public static bool ChangeActiveStrategyOfBuilding(HexCoordinates coordinates, TroopType type, bool newValue)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null)
            {
                if (cell.Structure is ProtectedBuilding)
                {
                    int index = ((ProtectedBuilding)cell.Structure).TroopInventory.Strategy.FindIndex(tpl => tpl.Item1 == type);
                    ((ProtectedBuilding)cell.Structure).TroopInventory.Strategy[index] = new Tuple<TroopType, bool>(type, newValue);
                    return true;
                }
            }
            return false;
        }

        public static bool ChangeActiveStrategyOfPlayer(String playerName, TroopType type, bool newValue)
        {
            Player player = GetPlayer(playerName);
            if (player != null)
            {
                int index = player.TroopInventory.Strategy.FindIndex(tpl => tpl.Item1 == type);
                player.TroopInventory.Strategy[index] = new Tuple<TroopType, bool>(type, newValue);
                return true;
            }
            return false;
        }

        public static bool MoveRessources(HexCoordinates originCoordinates, HexCoordinates destinationCoordinates, RessourceType type, int amount)
        {
            HexCell originCell = grid.GetCell(originCoordinates);
            HexCell destinationCell = grid.GetCell(destinationCoordinates);
            if (originCell != null && destinationCell != null)
            {
                if (originCell.Structure is InventoryBuilding && destinationCell.Structure is InventoryBuilding)
                {
                    InventoryBuilding origin = (InventoryBuilding)originCell.Structure;
                    InventoryBuilding destination = (InventoryBuilding)destinationCell.Structure;
                    if (origin.Inventory.GetRessourceAmount(type) >= amount && destination.Inventory.AvailableSpace(type) >= amount)
                    {
                        origin.Inventory.RemoveRessource(type, amount);
                        destination.Inventory.AddRessource(type, amount);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool UpdateRessourceLimit(HexCoordinates coordinates, RessourceType type, int newValue)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null)
            {
                if (cell.Structure is InventoryBuilding)
                {
                    InventoryBuilding building = (InventoryBuilding)cell.Structure;
                    if (newValue < 0)
                        return false;
                    building.Inventory.UpdateRessourceLimit(type, newValue);
                    return true;
                }
            }
            return false;
        }

        public static bool UpdateRefineryRecipe(HexCoordinates coordinates, RessourceType output)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null && cell.Structure is MultiRefineryBuilding)
            {
                MultiRefineryBuilding building = (MultiRefineryBuilding)cell.Structure;
                building.ChangeRecipe(output);
                ComputeConnectedStorages();
                return true;
            }
            return false;
        }

        public static bool UpdateMarketRessource(HexCoordinates coordinates, RessourceType type, bool isInput)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null)
            {
                if (cell.Structure is Market)
                {
                    Market market = (Market)cell.Structure;
                    if (isInput)
                        market.ChangeInputRecipe(type);
                    else
                        market.ChangeOutputRecipe(type);
                    ComputeConnectedStorages();
                    return true;
                }
            }
            return false;
        }

        public static bool UpdateBoostBaseResourceSelectionGuild(HexCoordinates coordinates, RessourceType type)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null && cell.Structure is GuildHouse guildHouse)
            {
                guildHouse.ChangeBoostBaseResource(type);
                return true;
            }

            return false;
        }

        public static bool UpdateBoostAdvancedResourceSelectionGuild(HexCoordinates coordinates, RessourceType type)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null && cell.Structure is GuildHouse guildHouse)
            {
                guildHouse.ChangeBoostAdvancedResource(type);
                return true;
            }

            return false;
        }

        public static bool UpdateBoostWeaponSelectionGuild(HexCoordinates coordinates, RessourceType type)
        {
            HexCell cell = grid.GetCell(coordinates);
            if (cell != null && cell.Structure is GuildHouse guildHouse)
            {
                guildHouse.ChangeBoostWeapon(type);
                return true;
            }

            return false;
        }

        public static string UpdateWeather(HexCoordinates coordinates)
        {

            // find nearby up-to-date weather
            //HexCell cell = grid.GetCell(coordinates);
            //for c in neighbouringCells
            //    if weather(c).isCurrent
            //        bFound = true;
            //if !bFound
            return Weather(coordinates);

        }


        private const string API_KEY = "88bcbc4c695622fbc96076f3cb6093d7";

        //from http://csharphelper.com/blog/2018/07/get-a-weather-forecast-from-openweathermap-org-in-c/
        public static string Weather(HexCoordinates coordinates)
        {

            string[] exclude_strings = { "current", "minutely", "hourly", "daily", "alerts" };
            Vector3 pos = grid.GetPosition(coordinates);
            float lon = pos.x;
            float lat = pos.y;
            string weather = "";
            // Create a web client.
            using (WebClient client = new())
            {
                // Get the response string from the URL.
                try
                {
                    //WebRequest request = WebRequest.Create(url);
                    //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    weather = client.DownloadString($"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={API_KEY}&units=metric&mode=xml"); //&exclude={part}
                    //Debug
                    DisplayForecast(weather);
                }
                catch (WebException ex)
                {
                    DisplayError(ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unknown error\n" + ex.Message);
                }
            }
            return weather;
        }

        // Display the forecast.
        public static void DisplayForecast(string weather)
        {
            // Load the response into an XML document.


            XmlDocument xml_doc = new();

            Console.WriteLine("Test: \n" + weather);
            try
            {
                xml_doc.LoadXml(weather);


                // Get the city, country, latitude, and longitude.
                XmlNode loc_node = xml_doc.SelectSingleNode("current/city");
                String txtCity = loc_node.Attributes["name"].Value;
                Console.WriteLine($"textCity: {txtCity}");
                String txtId = loc_node.Attributes["id"].Value;
                Console.WriteLine($"txtId: {txtId}");
                String txtCountry = loc_node.SelectSingleNode("country").InnerText;
                Console.WriteLine($"txtCountry: {txtCountry}");
                XmlNode geo_node = loc_node.SelectSingleNode("coord");
                String txtLat = geo_node.Attributes["lat"].Value;
                Console.WriteLine($"txtLat: {txtLat}");
                String txtLong = geo_node.Attributes["lon"].Value;
                Console.WriteLine($"txtLong: {txtLong}");

                char degrees = (char)176;

                XmlNode time_node = xml_doc.SelectSingleNode("current/lastupdate");
                // Get the time in UTC.
                DateTime time = DateTime.Parse(time_node.Attributes["value"].Value, null, DateTimeStyles.AssumeUniversal);

                // Get the temperature.
                XmlNode temp_node = xml_doc.SelectSingleNode("current/temperature");
                string temp = temp_node.Attributes["value"].Value;

                // Get the weather.
                XmlNode weather_node = xml_doc.SelectSingleNode("current/weather");
                string weather_value = weather_node.Attributes["value"].Value;
                string weather_id = weather_node.Attributes["number"].Value;

                // Get percipitation
                XmlNode precipitation_node = xml_doc.SelectSingleNode("current/precipitation");
                string precipitation = precipitation_node.Attributes["mode"].Value;

                // Get the wind.
                XmlNode wind_node = xml_doc.SelectSingleNode("current/wind");
                string wind = wind_node.SelectSingleNode("speed").Attributes["value"].Value;


                Console.WriteLine($"DayOfWeek: {time.DayOfWeek}");
                Console.WriteLine($"TimeString: {time.ToShortTimeString()}");
                Console.WriteLine($"temp + degrees: {temp + degrees}");
                Console.WriteLine($"weather: {weather_value + weather_id}");
                Console.WriteLine($"wind: {wind} m/s");
                Console.WriteLine($"precipitation: {precipitation}");
            }
            catch (XmlException e)
            {
                Console.WriteLine("XML error: " + e.Message);
            }
        }

        // Display an error message.
        private static void DisplayError(WebException exception)
        {
            try
            {
                StreamReader reader = new(exception.Response.GetResponseStream());
                XmlDocument response_doc = new();
                response_doc.LoadXml(reader.ReadToEnd());
                XmlNode message_node = response_doc.SelectSingleNode("//message");
                Console.WriteLine("DisplayError: " + message_node.InnerText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisplayError Unknown error\n" + ex.Message);
            }
        }
        public static void UpdateTime(DateTime time)
        {
            int hours = time.Hour;
            int minutes = time.Minute;
            //int seconds = time.Second;

            if (hours >= 22 || hours <= 6)
            {
                _currentTime = "Night";
            }
            else if (hours > 6 && hours < 12)
            {
                _currentTime = "Dawn";
            }
            else if (hours >= 12 && hours <= 17)
            {
                _currentTime = "Day";
            }
            else if (hours > 17 && hours < 22)
            {
                _currentTime = "Dusk";
            }
        }
        #endregion

        public static void DoTick()
        {
            if (initialized)
            {
                UpdateTime(DateTime.Now);
               // UpdateTime(time);
                foreach (Ressource ressource in ressources)
                {
                    if (_currentTime == "Day")
                    {
                        ressource.DoTick();
                    }
                    else if (_currentTime == "Dawn")
                    {
                        ressource.DoDawnTick();
                    }
                    else if (_currentTime == "Dusk")
                    {
                        ressource.DoDuskTick();
                    }
                    else if (_currentTime == "Night")
                    {
                        ressource.DoNightTick();
                    }
                }
                foreach (Building building in buildings)
                {
                    building.DoTick();
                    /*
                    for(HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                    {
                        HexCell neighbor = building.Cell.GetNeighbor(d);
                        if (neighbor != null && neighbor.Structure != null && neighbor.Structure is Ressource)
                        {
                            if(((Ressource)neighbor.Structure).Progress == 0)
                                this.AddStructureToList(neighbor.Structure);
                        }
                    }
                    */
                }
                foreach (Collectable collectable in collectables) 
                {
                    collectable.DoTick();
                   
                    
                }
                foreach (Tribe tribe in Tribes)
                {
                    foreach (TribeBuff buff in tribe.Buffs)
                    {
                        buff.DoTick();
                    }
                    List<TribeBuff> inactives = tribe.RemoveBuffs();
                    foreach (TribeBuff buff in inactives)
                    {
                        tribe.RemoveBuff(buff);
                    }
                }
                foreach (Cart cart in carts)
                {
                    cart.DoTick();
                }
                foreach(Player p in Players)
                {
                    p.DoTick();
                    //if (typeof(AIPlayer).IsAssignableFrom(p.GetType()))
                    //{
                    //    AIPlayer pl = (AIPlayer)p;
                    //    pl.DoTick();
                    //} 
                }
                updateCellStructures();
            }

        }

        public static void updateCellStructures() 
        {
            
            if (initialized)
            {
                foreach (HexCell cell in grid.cells)
                {
                    //if (cell.Structure is Empty) 
                    if(cell.Structure == null)
                    {
                        Double r2 = random.NextDouble();
                        Double r = random.NextDouble();
                        if (r < 0.2) 
                        {
                            switch (cell.Data.Biome)
                            {
                                case HexCellBiome.FOREST:
                                    {

                                        if (r < 0.05)
                                        {
                                            cell.Structure = new Loot(cell);
                                            //Console.Write("Loot planted on " + cell.Position.x + ";" + cell.Position.y + ";" + cell.Position.z);
                                        }
                                        if (r > 0.05 && r < 0.1) 
                                        {
                                            cell.Structure = new WoodBuff(cell);
                                        }
                                        else
                                        {
                                            cell.Structure = new Tree(cell, Progress);
                                            //Console.Write("Tree planted on " + cell.Position.x + ";" + cell.Position.y + ";" + cell.Position.z);
                                        }
                                        break;
                                    }

                                case HexCellBiome.ROCK:
                                    {
                                        if (r < 0.05)
                                        {
                                            cell.Structure = new Loot(cell);
                                        }
                                        if (r > 0.05 && r < 0.1) 
                                        {
                                            cell.Structure = new StoneBuff(cell);
                                        }
                                        else
                                        {
                                            cell.Structure = new Rock(cell, Progress);
                                        }
                                        break;
                                    }
                                case HexCellBiome.WATER:
                                    {

                                        if (r < 0.1)
                                            cell.Structure = new Fish(cell, Progress);
                                        break;
                                    }
                                case HexCellBiome.SCRUB:
                                    {
                                        if (r < 0.05)
                                        {
                                            cell.Structure = new Loot(cell);
                                        }
                                        if(r > 0.05 && r < 0.1) 
                                        {
                                            cell.Structure = new WoodBuff(cell);
                                        }
                                        else
                                        {
                                            cell.Structure = new Scrub(cell, Progress);
                                        }
                                        break;
                                    }
                                case HexCellBiome.GRASS:
                                    {
                                        if (r < 0.05)
                                        {
                                            cell.Structure = new Loot(cell);
                                        }
                                        if (r > 0.05 && r < 0.1)
                                        {
                                            cell.Structure = new CowBuff(cell);
                                        }
                                        if (r > 0.1 && r < 0.5)
                                        {
                                            cell.Structure = new Cow(cell, Progress); 
                                        }
                                        else
                                            cell.Structure = new Grass(cell, Progress);
                                        break;
                                    }
                                case HexCellBiome.CITY:
                                    {
                                        if (r > 0.05 && r < 0.1)
                                        {
                                            cell.Structure = new KnightBuff(cell, 3);
                                        }
                                        if (r > 0.1 && r < 0.15)
                                        {
                                            cell.Structure = new ArcherBuff(cell, 3);
                                        }
                                        if (r > 0.15 && r < 0.2)
                                        {
                                            cell.Structure = new ScoutBuff(cell, 3);
                                        }
                                        if (r > 0.2 && r < 0.25)
                                        {
                                            cell.Structure = new SEBuff(cell, 3);
                                        }
                                        if (r > 0.25 && r < 0.3)
                                        {
                                            cell.Structure = new SpearmanBuff(cell, 3);
                                        }
                                        break;
                                    }
                                case HexCellBiome.COAL:
                                    {
                                        if (r < 0.05)
                                        {
                                            cell.Structure = new Loot(cell);
                                        }
                                        if (r > 0.05 && r < 0.1)
                                        {
                                            cell.Structure = new CoalBuff(cell);
                                        }
                                        else
                                        {
                                            cell.Structure = new CoalOre(cell, Progress);
                                        }
                                        break;
                                    }
                                case HexCellBiome.CROP:
                                    {
                                        if(r < 0.05) 
                                        {
                                            cell.Structure = new Loot(cell);
                                        }
                                        if (r > 0.05 && r < 0.1) 
                                        {
                                            cell.Structure = new WheatBuff(cell);
                                        }
                                        else if (r > 0.2 && r < 0.4)
                                            cell.Structure = new Wheat(cell, Progress);
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            if (cell.Structure != null)
                            {
                                AddStructureToList(cell.Structure);
                            }
                        }
                    }
                }
            }
        }
        public static bool PrioChange(Tribe tribe, int prio, bool increase)
        {
            List<Building> aps = tribe.CurrentBuildings[typeof(AssemblyPoint)];
            if (increase)
            {
                if (prio > 1)
                {
                    foreach (AssemblyPoint ap in aps)
                    {
                        if (ap.priority == prio) ap.priority -= 1;
                        else if (ap.priority == prio - 1) ap.priority += 1;
                    }
                    return true;
                }
            }
            else
            {
                if (prio < aps.Count())
                {
                    foreach (AssemblyPoint ap in aps)
                    {
                        if (ap.priority == prio) ap.priority += 1;
                        else if (ap.priority == prio - 1) ap.priority -= 1;
                    }
                    return true;
                }
            }
            return false;
        }

        #region Compute Connected Storages

        public static void ComputeConnectedStorages()
        {
            foreach (Building building in buildings)
            {
                if (building is Road)
                {
                    ((Road)building).connectedStorages.Clear();
                }
                if (building is InventoryBuilding)
                {
                    ((InventoryBuilding)building).ConnectedInventories.Clear();
                }
            }
            foreach (Building building in buildings)
            {

                if (building is InventoryBuilding)
                {
                    for (HexDirection dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
                    {
                        HexCell neighbor = building.Cell.GetNeighbor(dir);
                        if (neighbor != null && neighbor.Structure is Road && ((Road)neighbor.Structure).HasBuilding(dir.Opposite()))
                        {
                            Road road = (Road)neighbor.Structure;
                            ComputeConnectedStorages(road, (InventoryBuilding)building, dir.Opposite(), road.Level, 0);
                        }
                    }
                }
            }
        }

        public static void ComputeConnectedStorages(Road current, InventoryBuilding origin, HexDirection direction, int minRoadLevel, int depth)
        {
            //Check if the tribe is already in the current dictionary and add it if it isn't
            if (!current.connectedStorages.ContainsKey(origin.Tribe))
                current.connectedStorages.Add(origin.Tribe, new Dictionary<InventoryBuilding, Tuple<HexDirection, int, int>>());

            //Check if the origin Building already has an entry in the dictionary
            if (current.connectedStorages[origin.Tribe].ContainsKey(origin))
            {
                //An entry of the origin Building already exists. Check if the current Route has a better flowrate.
                if (FlowRate(current.connectedStorages[origin.Tribe][origin].Item2, current.connectedStorages[origin.Tribe][origin].Item3) >= FlowRate(minRoadLevel, depth))
                {
                    return;
                }
                else
                {
                    current.connectedStorages[origin.Tribe][origin] = new Tuple<HexDirection, int, int>(direction, Mathf.Min(minRoadLevel, current.Level), depth);
                }
            }
            else
            {
                current.connectedStorages[origin.Tribe].Add(origin, new Tuple<HexDirection, int, int>(direction, Mathf.Min(minRoadLevel, current.Level), depth));
            }

            //Look at the neighbors of the current road
            for (HexDirection dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
            {
                HexCell neighbor = current.Cell.GetNeighbor(dir);
                //Propogate the route to origin through the roads
                if (neighbor != null && neighbor.Structure is Road && current.HasRoad(dir))
                {
                    ComputeConnectedStorages((Road)neighbor.Structure, origin, dir.Opposite(), Mathf.Min(minRoadLevel, current.Level), depth + 1);
                }

                //Add the entry to a connected Building
                if (neighbor != null && neighbor.Structure is InventoryBuilding && current.HasBuilding(dir) && neighbor.Structure != origin && ((InventoryBuilding)neighbor.Structure).Tribe == origin.Tribe)
                {
                    InventoryBuilding inventoryBuilding = (InventoryBuilding)neighbor.Structure;
                    if (inventoryBuilding.ConnectedInventories.ContainsKey(origin))
                    {
                        if (FlowRate(inventoryBuilding.ConnectedInventories[origin].Item2, inventoryBuilding.ConnectedInventories[origin].Item3) < FlowRate(minRoadLevel, depth + 1))
                        {
                            inventoryBuilding.ConnectedInventories[origin] = new Tuple<HexDirection, int, int>(dir.Opposite(), minRoadLevel, depth + 1);
                        }
                    }
                    else
                    {
                        inventoryBuilding.ConnectedInventories.Add(origin, new Tuple<HexDirection, int, int>(dir.Opposite(), minRoadLevel, depth + 1));
                    }

                    //Init allowed Ressources for origin Building
                    if (!origin.AllowedRessources.ContainsKey(inventoryBuilding))
                    {
                        origin.AllowedRessources.Add(inventoryBuilding, new Dictionary<RessourceType, bool>());
                    }
                    foreach (RessourceType ressourceType in origin.Inventory.Outgoing)
                    {
                        if (inventoryBuilding.Inventory.Incoming.Contains(ressourceType))
                        {
                            if (!origin.AllowedRessources[inventoryBuilding].ContainsKey(ressourceType))
                            {
                                origin.AllowedRessources[inventoryBuilding].Add(ressourceType, true);
                            }
                        }
                    }

                }

            }
        }

        private static float FlowRate(int minRoadLevel, int depth)
        {
            return (float)(minRoadLevel * 5) / (float)(depth * 2);
        }
        #endregion

    }
}
