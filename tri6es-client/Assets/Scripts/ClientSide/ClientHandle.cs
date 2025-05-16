using System.Collections.Generic;
using UnityEngine;
using Shared.HexGrid;
using Shared.Communication;
using Shared.DataTypes;
using Shared.Game;
using Shared.Structures;
using System;
using TMPro;

public class ClientHandle : MonoBehaviour
{
    static HexMeshGrid meshGrid;

    static UIManager uiManager;
    static GPS gps;

    static LoadManager loadManager;
    public static float loadingProgress;
    public static bool isDone = false;

    public static int head;

    public static void Welcome(Packet packet)
    {
        uiManager = FindObjectOfType<UIManager>();
        gps = FindObjectOfType<GPS>();

        loadManager = FindObjectOfType<LoadManager>();
        loadingProgress = 0f;

        string msg = packet.ReadString();
        int myid = packet.ReadInt();

        Debug.Log($"Message from Server: {msg}");
        Client.instance.myid = myid;
        ClientSend.WelcomeRecieved();
        loadingProgress += (1f / 4f);
    }

    public static void Ping(Packet packet)
    {
        long ping = packet.ReadLong();
        int idCheck = packet.ReadInt();

        if(idCheck != Client.instance.myid)
        {
            Debug.Log($"Message was not meant for this Client!");
        }
        else
        {
            Debug.Log($"Ping to Server is: {ping}ms");
        }
    }

    public static void InitGameLogic(Packet packet)
    {
        HexGrid grid = packet.ReadHexGrid();

        GameLogic.Init(grid);
        loadingProgress += (1f / 4f);

        meshGrid = FindObjectOfType<HexMeshGrid>();
        meshGrid.SetGrid(grid);
        loadingProgress += (1f / 4f);

        HexMapCamera hexMapCamera = FindObjectOfType<HexMapCamera>();
        hexMapCamera.SetGrid(grid);

        //HexGrid takes player??
        string playerName = packet.ReadString();
        int tribeId = packet.ReadByte();
        HexCoordinates playerPos = packet.ReadHexCoordinates();
        TroopInventory troopInventory = packet.ReadTroopInventory();
        List<List<Quest>> questlines = packet.ReadQuestlines();
        byte AvatarId = packet.ReadByte();

        Tribe tribe = GameLogic.GetTribe(tribeId);

        Player ownPlayer = GameLogic.GetPlayer(playerName);
        if (ownPlayer == null)
        {
            ownPlayer = GameLogic.AddPlayer(playerName, tribe);
        }
        ownPlayer.Tribe = tribe;
        ownPlayer.Position = playerPos;
        ownPlayer.TroopInventory = troopInventory;
        ownPlayer.Quests = questlines;
        ownPlayer.Avatar = AvatarId;

        // update quest display
        List<Quest> active = new List<Quest>();
        List<Quest> completed = new List<Quest>();
        foreach (List<Quest> line in questlines)
        {
            // find first active quest
            int index = line.FindIndex(quest => quest.isActive);
            if (index >= 0)
            {
                // add active quest
                active.Add(line[index]);
            }
            else
            {
                // no active quest is all quests completed
                index = line.Count;
            }

            // add completed (previous in line)
            for (int i = 0; i < index; i++)
            {
                completed.Add(line[i]);
            }
        }
        changeDisplayedQuests(active, completed);

        Client.instance.Player = ownPlayer;
        meshGrid.UpdateActiveChunks(ownPlayer.Position);

        loadingProgress += (1f / 4f);

        int count = packet.ReadInt();
        for (int i = 0; i < count; i++)
        {
            ReceivePlayer(packet);
        }
        
        // Tribe data
        int numTribes = packet.ReadInt();
        for (int i = 0; i < numTribes; ++i)
        {
            tribeId = packet.ReadByte();
            tribe = GameLogic.GetTribe(tribeId);
            
            int numResearches = packet.ReadInt();
            for (int j = 0; j < numResearches; ++j)
            {
                int researchCode = packet.ReadInt();
                tribe.AddResearch(new Research(researchCode));
            }
        }
        
        // Guild data
        int numGuilds = packet.ReadInt();
        for (int i = 0; i < numGuilds; ++i)
        {
            byte guildId = packet.ReadByte();
            byte foundingTribeId = packet.ReadByte();
            Tribe foundingTribe = GameLogic.GetTribe(foundingTribeId);
            Guild guild = GameLogic.GetGuild(guildId);
            if (guild == null)
            {
                guild = GameLogic.AddGuild(guildId, foundingTribe);
            }

            guild.Level = packet.ReadInt();
            guild.Inventory = packet.ReadGuildInventory();
            
            SortedDictionary<RessourceType, int> progressMap = guild.GetCurrentProgressMap();
            int mapSize = packet.ReadInt();
            for (int j = 0; j < mapSize; ++j)
            {
                int key = packet.ReadInt();
                int value = packet.ReadInt();
                progressMap[(RessourceType) key] = value;
            }

            int numMembers = packet.ReadInt();
            for (int j = 0; j < numMembers; ++j)
            {
                byte memberId = packet.ReadByte();
                Tribe memberTribe = GameLogic.GetTribe(memberId);
                if (!guild.Members.Contains(memberTribe))
                {
                    guild.Members.Add(memberTribe);
                }
                memberTribe.GuildId = guildId;  // Includes FoundingMember
            }
        }

        UpdateFireColors(ownPlayer);

        if (ownPlayer.Tribe != null)
        {
            if (ownPlayer.Tribe.HasGuild())
            {
                uiManager.AddChatFilterGuild();
            }
            
            if (ownPlayer.Tribe.HasResearched(Research.AREA_RECONNAISSANCE))
            {
                meshGrid.IncreasePlayerFov(ownPlayer);
            }
        }

        uiManager.OpenTutorial();
        isDone = true;
    }

    /// <summary>
    /// Sets the fire colors of HQs from the player's perspective.
    ///     - White: tribes with no guild
    ///     - Green: tribes in the same guild as the player's tribe
    ///     - Red:   tribes in a different guild
    /// </summary>
    private static void UpdateFireColors(Player player)
    {
        foreach (Tribe tribe in GameLogic.Tribes)
        {
            Color fireColor;
            if (tribe.HasNoGuild())
            {
                // White if a tribe does not belong to a guild.
                fireColor = new Color(1f, 1f, 1f, 1f);
            }
            else if (player.Tribe != null && player.Tribe.GuildId == tribe.GuildId)
            {
                // Green if the tribe is part of the player's guild.
                fireColor = new Color(0f, 1f, 0f, 1f);
            }
            else
            {
                // Red if the tribe belongs to a different guild.
                fireColor = new Color(1f, 0f, 0f, 1f);
            }

            meshGrid.UpdateFireColor(tribe.HQ.Cell, fireColor);
        }
    }

    public static void ReceiveGameTick(Packet packet)
    {
        if (GameLogic.initialized == true)
        {
            GameLogic.DoTick();
            meshGrid.Refresh();
            uiManager.UpdateUIElements();
            gps.StartLocation();
        }
    }
    public static void HandlePrioChange(Packet packet)
    {
        byte tribeID = packet.ReadByte();
        int prio = packet.ReadInt();
        bool increase = packet.ReadBool();
        GameLogic.PrioChange(GameLogic.GetTribe(tribeID),prio, increase);
        uiManager.UpdateUIElements();
    }

    public static void ApplyBuild(Packet packet)
    {
        HexCoordinates coords = packet.ReadHexCoordinates();
        Type buildingType = packet.ReadType();
        byte tribeID = packet.ReadByte();
        String playerName = packet.ReadString();
        Player player = GameLogic.GetPlayer(playerName);

        Feedback feedback = packet.ReadFeedback();
        feedback.coordinates = coords;
        feedback.type = buildingType;

        if (player.GetType().IsAssignableFrom(typeof(AIPlayer))){
            GameLogic.ApplyBuild(coords, buildingType, GameLogic.GetTribe(tribeID));
            meshGrid.BuildBuilding(coords);
            return;
        }

        uiManager.openFeedback(feedback);
        if (!feedback.successfull) return; 

        GameLogic.ApplyBuild(coords, buildingType, GameLogic.GetTribe(tribeID));
        meshGrid.BuildBuilding(coords);
        if (player.Tribe == GameLogic.GetTribe(tribeID) && player == Client.instance.Player)
        {
            if (player.hasActiveBuildingQuest(buildingType))
            {
                ClientSend.sendBuildingQuestProgress(buildingType);
            }

            if (player.Position == coords)
            {
                // Building was placed into the same cell in which the player stands -> move player next to the building
                meshGrid.UpdatePlayerPosition(player);
            }
        }
    }

    public static void UpgradeBuilding(Packet packet)
    {
        HexCoordinates coords = packet.ReadHexCoordinates();
        Feedback feedback = packet.ReadFeedback();

        HexCell cell = GameLogic.grid.GetCell(coords);

        feedback.coordinates = coords;
        uiManager.openFeedback(feedback);
        
        if (!feedback.successfull)
        {
            return;
        }

        if (cell == null || !(cell.Structure is Building))
        {
            return;
        }
        GameLogic.ApplyUpgrade(coords, GameLogic.GetTribe(((Building)cell.Structure).Tribe));
        meshGrid.BuildBuilding(cell);
    }

    public static void RepairBuilding(Packet packet)
    {
        HexCoordinates coords = packet.ReadHexCoordinates();

        HexCell cell = GameLogic.grid.GetCell(coords);
        if (cell == null || !(cell.Structure is Building))
            return;
        GameLogic.ApplyRepairBuilding(coords, GameLogic.GetTribe(((Building)cell.Structure).Tribe));
        meshGrid.BuildBuilding(cell);
    }
    public static void HandleSalvage(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        HexCell cell = GameLogic.grid.GetCell(coordinates);
        if(cell==null || !(cell.Structure is Ruin))return;
        GameLogic.ApplySalvage(coordinates, GameLogic.GetTribe(((Building)cell.Structure).Tribe));
        meshGrid.BuildBuilding(cell);
    }

    public static void ApplyBuildHQ(Packet packet)
    {
        HexCoordinates coords = packet.ReadHexCoordinates();

        Headquarter hq = new Headquarter();

        GameLogic.ApplyBuildHQ(coords, hq);

        meshGrid.BuildBuilding(coords);
    }

    public static void CreateGuild(Packet packet)
    {
        byte foundingTribeId = packet.ReadByte();
        Feedback feedback = packet.ReadFeedback();
        
        uiManager.openFeedback(feedback);

        if (feedback.successfull)
        {
            Tribe foundingTribe = GameLogic.GetTribe(foundingTribeId);
            GameLogic.CreateGuild(foundingTribe);
            uiManager.AddChatFilterGuild();
        }
    }

    public static void LeaveGuild(Packet packet)
    {
        byte leavingTribeId = packet.ReadByte();
        Feedback feedback = packet.ReadFeedback();
        
        uiManager.openFeedback(feedback);

        if (feedback.successfull)
        {
            Tribe leavingTribe = GameLogic.GetTribe(leavingTribeId);
            GameLogic.LeaveGuild(leavingTribe);
            uiManager.RemoveChatFilterGuild();

            GuildHouse guildHouse = leavingTribe.GetGuildHouse();
            if (guildHouse != null && guildHouse.Level != 1)
            {
                // Update GuildHouse building if necessary (should be level 1 if no guild)
                GameLogic.ChangeGuildHouseLevel(leavingTribe, 1);
                meshGrid.BuildBuilding(guildHouse.Cell);
            }
        }
    }

    public static void JoinGuild(Packet packet)
    {
        byte guildToJoinId = packet.ReadByte();
        byte joiningTribeId = packet.ReadByte();
        Feedback feedback = packet.ReadFeedback();

        uiManager.openFeedback(feedback);
        
        if (feedback.successfull)
        {
            Tribe joiningTribe = GameLogic.GetTribe(joiningTribeId);
            GameLogic.JoinGuild(guildToJoinId, joiningTribe);
            uiManager.AddChatFilterGuild();
            
            Guild guild = GameLogic.GetGuild(guildToJoinId);
            GuildHouse guildHouse = joiningTribe.GetGuildHouse();
            if (guildHouse != null && guildHouse.Level != guild.Level)
            {
                // Update GuildHouse building if necessary (should be same level as guild)
                GameLogic.ChangeGuildHouseLevel(joiningTribe, (byte)guild.Level);
                meshGrid.BuildBuilding(guildHouse.Cell);
            }
        }
    }

    public static void GuildDonation(Packet packet)
    {
        byte guildId = packet.ReadByte();
        byte tribeId = packet.ReadByte();
        
        Guild guild = GameLogic.GetGuild(guildId);
        Tribe tribe = GameLogic.GetTribe(tribeId);
        SortedDictionary<RessourceType, int> progressMap = guild.GetCurrentProgressMap();
        Dictionary<RessourceType, int> actualDonationMap = new Dictionary<RessourceType, int>();

        int mapSize = packet.ReadInt();
        for (int i = 0; i < mapSize; ++i)
        {
            RessourceType key = (RessourceType) packet.ReadInt();
            int newValue = packet.ReadInt();

            int oldValue = progressMap[key];
            actualDonationMap[key] = Math.Abs(oldValue - newValue);
            
            progressMap[key] = newValue;
        }

        // Remove donation from inventory
        tribe.tribeInventory.ApplyRecipe(actualDonationMap);

        if (guild.IsProgressReached())
        {
            guild.LevelUp();
        }
        
        // TODO: not the right place to construct it, I think (do on server?)
        // == Feedback ===========================================
        Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage);
        feedback.successfull = true;
        feedback.message = "Donated ";
        foreach (KeyValuePair<RessourceType, int> pair in actualDonationMap)
        {
            if (pair.Value != 0)
            {
                feedback.message += $"{pair.Value} {pair.Key}, ";
            }
        }
        // Remove the last comma
        feedback.message = feedback.message.Remove(feedback.message.Length - 2);
        // Show on screen
        uiManager.openFeedback(feedback);
    }

    public static void DepositToGuild(Packet packet)
    {
        byte guildId = packet.ReadByte();
        byte tribeId = packet.ReadByte();
        
        Guild guild = GameLogic.GetGuild(guildId);
        Tribe tribe = GameLogic.GetTribe(tribeId);

        Feedback feedback = packet.ReadFeedback();

        SortedDictionary<RessourceType, int> currentResources = guild.Inventory.Resources;
        Dictionary<RessourceType, int> actualDepositMap = new Dictionary<RessourceType, int>();

        int mapSize = packet.ReadInt();
        for (int i = 0; i < mapSize; ++i)
        {
            RessourceType key = (RessourceType) packet.ReadInt();
            int newValue = packet.ReadInt();

            int oldValue = currentResources[key];
            actualDepositMap[key] = Math.Abs(oldValue - newValue);

            currentResources[key] = newValue;
        }
        
        tribe.tribeInventory.ApplyRecipe(actualDepositMap);
        
        uiManager.openFeedback(feedback);
    }
    
    public static void WithdrawalFromGuild(Packet packet)
    {
        byte guildId = packet.ReadByte();
        byte tribeId = packet.ReadByte();
        
        Guild guild = GameLogic.GetGuild(guildId);
        Tribe tribe = GameLogic.GetTribe(tribeId);

        Feedback feedback = packet.ReadFeedback();

        SortedDictionary<RessourceType, int> currentResources = guild.Inventory.Resources;

        int mapSize = packet.ReadInt();
        for (int i = 0; i < mapSize; ++i)
        {
            RessourceType key = (RessourceType) packet.ReadInt();
            int newValue = packet.ReadInt();

            int oldValue = currentResources[key];
            int withdrawalAmount = Math.Abs(oldValue - newValue);
            tribe.tribeInventory.AddRessource(key, withdrawalAmount);

            currentResources[key] = newValue;
        }
        
        uiManager.openFeedback(feedback);
    }

    public static void BoostBaseResourceSelectionGuild(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        RessourceType type = (RessourceType)packet.ReadByte();
        GameLogic.UpdateBoostBaseResourceSelectionGuild(coordinates, type);
        uiManager.ReloadStructure(coordinates);
    }

    public static void BoostAdvancedResourceSelectionGuild(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        RessourceType type = (RessourceType)packet.ReadByte();
        GameLogic.UpdateBoostAdvancedResourceSelectionGuild(coordinates, type);
        uiManager.ReloadStructure(coordinates);
    }

    public static void BoostWeaponSelectionGuild(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        RessourceType type = (RessourceType)packet.ReadByte();
        GameLogic.UpdateBoostWeaponSelectionGuild(coordinates, type);
        uiManager.ReloadStructure(coordinates);
    }

    // FIXME: this is unused? server sends full player instead of position update
    public static void HandlePositionUpdate(Packet packet)
    {
        string playerName = packet.ReadString();
        HexCoordinates coords = packet.ReadHexCoordinates();

        Player player = GameLogic.Players.Find(elem => elem.Name == playerName);
        if (player != null)
        {
            if (player == Client.instance.Player)
            {
                uiManager.RefreshCellHighlighting();
                uiManager.UpdateMap();
            }
        }
    }

    public static void ReceivePlayer(Packet packet)
    {
        string playerName = packet.ReadString();

        byte tribeId = packet.ReadByte();
        HexCoordinates playerPos = packet.ReadHexCoordinates();
        TroopInventory troopInventory = packet.ReadTroopInventory();
        byte AvatarId = packet.ReadByte();
       
        Tribe tribe = GameLogic.GetTribe(tribeId);

        Player player = GameLogic.GetPlayer(playerName);
        if (player == null)
        {
            player = GameLogic.AddPlayer(playerName, tribe);
            player.Position = playerPos;
            player.TroopInventory = troopInventory;
            player.Avatar = AvatarId;
            meshGrid.UpdatePlayerPosition(player);
        }
        else
        {   
            player.Tribe = tribe;
            HexCoordinates temppos = player.Position;
            player.Position = playerPos;
            player.TroopInventory = troopInventory;
            player.Avatar = AvatarId;
            if (player == Client.instance.Player)
            {
                meshGrid.UpdateActiveChunks(player.Position);
                uiManager.RefreshCellHighlighting();
                uiManager.UpdateMap();
                uiManager.StructureInfo();
                
                // Not ideal, because will also be called with every move the player makes.
                // Ideally call in handlers for create/join/leave tribe/guild,
                // but needs to be done here because no separate handlers for join/leave tribe.
                // Eg. see tri6es-server: ServerHandle::HandleJoinTribe() vs ServerHandle::HandleBuildHQ().
                UpdateFireColors(Client.instance.Player);
                
                // Again, not ideal...
                if (player.Tribe != null && player.Tribe.HasGuild())
                {
                    uiManager.AddChatFilterGuild();
                }
                else
                {
                    uiManager.RemoveChatFilterGuild();
                }

                if (player.Tribe != null && player.Tribe.HasResearched(Research.AREA_RECONNAISSANCE))
                {
                    meshGrid.IncreasePlayerFov(player);
                }
                else
                {
                    meshGrid.ResetPlayerFov(player);
                }
            }
            meshGrid.UpdatePlayerPosition(player);
            int distance = HexCoordinates.calcDistance(temppos, playerPos);
            if (player.hasActiveMovementQuest() && distance > 0)
            {
                ClientSend.sendMovementQuestProgress(distance);
            }
        }
    }

    public static void ReceiveTribe(Packet packet)
    {
        byte id = packet.ReadByte();
        HexCoordinates coords = packet.ReadHexCoordinates();

        HexCell cell = GameLogic.grid.GetCell(coords);
        if (cell == null)
        {
            return;
        }
        if (!(cell.Structure is Headquarter))
        {
            return;
        }

        Headquarter hq = (Headquarter) cell.Structure;

        Tribe tribe = GameLogic.GetTribe(id);
        if (tribe == null)
        {
            GameLogic.AddTribe(id, hq);
        }
        else
        {
            tribe.HQ = hq;
        }
    }

    public static void HandleMoveTroops(Packet packet)
    {
        HexCoordinates source = packet.ReadHexCoordinates();
        HexCoordinates dest = packet.ReadHexCoordinates();
        TroopType troopType = (TroopType)packet.ReadByte();
        int amount = packet.ReadInt();

        GameLogic.MoveTroops(source, dest, troopType, amount);
        uiManager.UpdateUIElements();
    }

    public static void HandleFight(Packet packet)
    {
        string playerName = packet.ReadString();
        HexCoordinates attacker = packet.ReadHexCoordinates();
        HexCoordinates defender = packet.ReadHexCoordinates();
        Feedback feedback = packet.ReadFeedback();

        feedback.playername = playerName;

        GameLogic.Fight(GameLogic.GetPlayer(playerName), attacker, defender);
        meshGrid.UpdateCell(defender);

        //TODO: feedbackmessage should change according to the user
        uiManager.openFeedback(feedback);
        
    }

    public static void HandleWeather(Packet packet)
    {
        //here

        HexCoordinates coordinates = packet.ReadHexCoordinates();
        Debug.Log($"Coordinates: " + coordinates);
        string weather = packet.ReadString();
        Debug.Log($"Weather from server: {weather}");
        Console.WriteLine($"Weather from server: {weather}");
        GameLogic.SetWeather(coordinates, weather);
    }

    public static void HandleHarvest(Packet packet)
    {
        string playerName = packet.ReadString();
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        Feedback feedback = packet.ReadFeedback();
        feedback.coordinates = coordinates;
        
        
        Player player = GameLogic.GetPlayer(playerName);
        Debug.Log("Name: "+player.Name);
        //Check if this player is the client
        if (Client.instance.Player.Name == player.Name) 
        {
            if (player.hasActiveRessourceQuest(coordinates))
            {
                ClientSend.sendRessourceQuestProgress(coordinates);
            }

            uiManager.openFeedback(feedback);
            
        }
        if (feedback.successfull)
        {
            Debug.Log(feedback.resource);
            if (player.GetType().IsAssignableFrom(typeof(AIPlayer)))  meshGrid.harvestRessourceCallback(feedback.coordinates, feedback.successfull, feedback.resource, feedback.quantity);
            GameLogic.Harvest(playerName, coordinates, ref feedback);
        }
    }
    public static void HandleCollect(Packet packet) 
    {
        Console.WriteLine("Client.HandleCollect wird ausgeführt");
        string playerName = packet.ReadString();
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        // Feedback feedback = packet.ReadFeedback();
        // feedback.coordinates = coordinates;

        Player player = GameLogic.GetPlayer(playerName);
        Feedback temporary = new Feedback("Remove after collect fixed");
        GameLogic.Collect(playerName, coordinates, ref temporary);
        //if (Client.instance.Player.Name == player.Name)
        //{
        //    Feedback temporary = new Feedback("Remove after collect fixed");
        //    GameLogic.Collect(playerName, coordinates, ref temporary);   
        //}
    }

    public static void HandleResearch(Packet packet)
    {
        byte tribeId = packet.ReadByte();
        int researchCode = packet.ReadInt();
        Feedback feedback = packet.ReadFeedback();

        uiManager.openFeedback(feedback);

        if (feedback.successfull)
        {
            Tribe tribe = GameLogic.GetTribe(tribeId);
            Research research = new Research(researchCode);
            GameLogic.ApplyResearch(tribe, research);

            if (research.Code == Research.AREA_RECONNAISSANCE && tribe.HasResearched(Research.AREA_RECONNAISSANCE))
            {
                meshGrid.IncreasePlayerFov(Client.instance.Player);
            }
        }
    }

    public static void HandleChangeAllowedRessource(Packet packet)
    {
        HexCoordinates originCoordinates = packet.ReadHexCoordinates();
        HexCoordinates destinationCoordinates = packet.ReadHexCoordinates();
        RessourceType ressourceType = (RessourceType)packet.ReadByte();
        bool newValue = packet.ReadBool();

        GameLogic.ChangeAllowedRessource(originCoordinates, destinationCoordinates, ressourceType, newValue);
        uiManager.UpdateUIElements();
    }

    public static void HandleChangeTroopRecipeOfBarracks(Packet packet)
    {
        HexCoordinates barracks = packet.ReadHexCoordinates();
        TroopType troopType = (TroopType)packet.ReadByte();
        GameLogic.ChangeTroopRecipeOfBarracks(barracks, troopType);
        uiManager.ReloadStructure(barracks);
    }

    public static void HandleChangeStrategyOfProtectedBuilding(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        int oldIndex = packet.ReadInt();
        int newIndex = packet.ReadInt();
        GameLogic.ChangeStrategyOfProtectedBuilding(coordinates, oldIndex, newIndex);
        uiManager.UpdateUIElements();
    }

    public static void HandleChangeStrategyOfPlayer(Packet packet)
    {
        string playerName = packet.ReadString();
        int oldIndex = packet.ReadInt();
        int newIndex = packet.ReadInt();
        GameLogic.ChangeStrategyOfPlayer(playerName, oldIndex, newIndex);
        uiManager.UpdateUIElements();
    }

    public static void HandleChangeStrategyActivePlayer(Packet packet)
    {
        string playerName = packet.ReadString();
        TroopType type = (TroopType)packet.ReadByte();
        bool newValue = packet.ReadBool();

        GameLogic.ChangeActiveStrategyOfPlayer(playerName, type, newValue);
        uiManager.UpdateUIElements();
    }

    public static void HandleChangeStrategyActiveBuilding(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        TroopType type = (TroopType)packet.ReadByte();
        bool newValue = packet.ReadBool();

        GameLogic.ChangeActiveStrategyOfBuilding(coordinates, type, newValue);
        uiManager.UpdateUIElements();
    }

    public static void HandleMoveRessources(Packet packet)
    {
        HexCoordinates originCoordinates = packet.ReadHexCoordinates();
        HexCoordinates destinationCoordinates = packet.ReadHexCoordinates();
        RessourceType type = (RessourceType)packet.ReadByte();
        int amount = packet.ReadInt();
        GameLogic.MoveRessources(originCoordinates, destinationCoordinates, type, amount);
        uiManager.UpdateUIElements();
    }

    public static void HandleChangeRessourceLimit(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        RessourceType type = (RessourceType)packet.ReadByte();
        int newValue = packet.ReadInt();
        GameLogic.UpdateRessourceLimit(coordinates, type, newValue);
        uiManager.UpdateUIElements();
    }

    public static void HandleRecipeChange(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        RessourceType ressource = (RessourceType) packet.ReadByte();
        GameLogic.UpdateRefineryRecipe(coordinates, ressource);
        uiManager.ReloadStructure(coordinates);
    }

    public static void HandleUpdateMarketRessource(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        RessourceType type = (RessourceType)packet.ReadByte();
        bool isInput = packet.ReadBool();
        GameLogic.UpdateMarketRessource(coordinates, type, isInput);
        // uiManager.UpdateUIElements();
        uiManager.ReloadStructure(coordinates);
    }

    public static void HandleDestroyBuilding(Packet packet)
    {
        HexCoordinates coordinates = packet.ReadHexCoordinates();
        Feedback feedback = packet.ReadFeedback();

        if (!feedback.successfull) return;

        List<HexCell> changedCells = new List<HexCell>();
        HexCell cell = GameLogic.grid.GetCell(coordinates);
        if (cell.Structure is ProtectedBuilding)
        {
            ProtectedBuilding building = (ProtectedBuilding) cell.Structure;
            changedCells = building.GetProtectedCells();
        }

        GameLogic.DestroyStructure(coordinates);        
        meshGrid.UpdateCell(coordinates);
        meshGrid.RefreshCellChunks(changedCells);

        uiManager.openFeedback(feedback);
    }

    public static void HandleChatHistory(Packet packet)
    {
        String history = packet.ReadString();
        uiManager.setChatHistory(history);
    }
    public static void HandleChatMessage(Packet packet)
    {
        String message = packet.ReadString();
        byte tribeId = packet.ReadByte();
        byte guildId = packet.ReadByte();
        string playerName = packet.ReadString();
        int chatType = packet.ReadInt();

        // (Check against ID_NO_TRIBE is not strictly necessary, because chat menu is not available for players 
        //  that do not belong to a tribe. But doesn't hurt to play it safe.)
        bool isTribeChat = chatType == 0 && tribeId != Tribe.ID_NO_TRIBE 
                                         && Client.instance.Player.Tribe.Id == tribeId;

        bool isGuildChat = chatType == 1 && guildId != Guild.ID_NO_GUILD 
                                         && Client.instance.Player.Tribe.GuildId == guildId;

        if (isTribeChat || isGuildChat)
        {
            uiManager.newChatMessage(message, playerName, chatType);
        }
    }

    public static void HandleQuests(Packet packet)
    {
        Player player = new Player("player");
        List<List<Quest>> allQuests = new List<List<Quest>>(player.Quests);
        List<uint> tmpAllQuests = new List<uint>(packet.ReadUIntArray());
        List<int> activeIndices = new List<int>();
        activeIndices = tmpAllQuests.ConvertAll(x => (int) x);
        List<uint> tmpSuccessfulQuests = new List<uint>(packet.ReadUIntArray());
        List<int> successfulQuests = new List<int>();
        successfulQuests = tmpSuccessfulQuests.ConvertAll(x => (int) x);
        List<Quest> activeQuests = new List<Quest>();
        List<Quest> completedQuests = new List<Quest>();
        int activeIndex = 0;
        for (int i = 0; i < allQuests.Count; i++)
        {

            for(int j = 0; j < allQuests[i].Count; j++)
            {
                    if(activeIndex < activeIndices.Count-1){
                        if (i == activeIndices[activeIndex] && j < activeIndices[activeIndex+1]){
                            completedQuests.Add(allQuests[i][j]);
                            Client.instance.Player.Quests[i][j].isActive = false;
                            if (Client.instance.Player.Quests[i].Count > j+1)
                            {
                                Client.instance.Player.Quests[i][j+1].isActive = true;
                            }
                        }
                        if (i == activeIndices[activeIndex] && j == activeIndices[activeIndex + 1])
                        {
                            allQuests[i][j].Progress = activeIndices[activeIndex + 2];
                            activeQuests.Add(allQuests[i][j]);

                        }
                    }

            }
            activeIndex += 3;
        }
        // display active and completed quests in different tabs
        changeDisplayedQuests(activeQuests, completedQuests);


        // upcoming quests of a quest line will not be displayed
        String name = packet.ReadString();
        HandleSuccessfulQuests(successfulQuests, name);
    }

    private static void changeDisplayedQuests(List<Quest> activeQuests, List<Quest> completedQuests)
    {
        Transform activeQ = uiManager.ActiveQuests.transform;
        Transform doneQ = uiManager.completedQuests.transform;
        if (activeQuests.Count != 0)
        {
            foreach (Transform child in activeQ)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Quest quest in activeQuests)
            {
                GameObject questprefab = Instantiate(uiManager.ActiveQuestPrefab, activeQ);
                questprefab.transform.GetChild(0).GetComponent<TMP_Text>().text = quest.GetTask();
                questprefab.transform.GetChild(1).GetComponent<TMP_Text>().text = "Reward: " + quest.GetReward();
            }
        }
        if (completedQuests.Count != 0)
        {
            foreach (Transform child in doneQ)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Quest quest in completedQuests)
            {
                GameObject questprefab = Instantiate(uiManager.ActiveQuestPrefab, doneQ);
                questprefab.transform.GetChild(0).GetComponent<TMP_Text>().text = quest.GetTask();
                questprefab.transform.GetChild(1).GetComponent<TMP_Text>().text = "Reward: " + quest.GetReward();
            }
        }
    }

    public static void HandleSuccessfulQuests(List<int> activeIndices, String name)
        {
        Player player = new Player("player");
        List<List<Quest>> allQuests = new List<List<Quest>>(player.Quests);
        List<Quest> successfulQuests = new List<Quest>();
        player = GameLogic.GetPlayer(name);
        for (int i = 0; i < activeIndices.Count; i+=2)
        {
            successfulQuests.Add(allQuests[activeIndices[i]][activeIndices[i+1]]);
        } //testen mit mehreren quests gleichzeitig abgeschlossen
       foreach (Quest quest in successfulQuests)
        {
            if (quest.Reward > 0) {
                player.Tribe.HQ.Inventory.AddRessource(quest.RewardType, quest.Reward);
            }
            uiManager.QuestSuccessPanel.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "Successfully completed the Quest: " + quest.GetTask();
            uiManager.QuestSuccessPanel.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = "Reward: " + quest.GetReward();
            uiManager.QuestSuccessPanel.SetActive(true);

        }

    }

   
}
