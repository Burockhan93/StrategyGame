using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Structures;
using Shared.Game;

namespace GameServer
{
    class ServerSend
    {
        public static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        public static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
            PacketBuffer.Add(packet.ToArray());
        }

        public static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }

            }
        }

        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                Server.StartPingTest();
                SendTCPData(toClient, packet);
            }
        }

        public static void Ping(int toClient, long ping)
        {
            //Send ping to Client
            using (Packet packet = new Packet((int)ServerPackets.ping))
            {
                packet.Write(ping);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }
        
        // Called when a player connects to the game.
        public static void InitGameLogic(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.initGameLogic))
            {
                //Send HexGrid
                packet.Write(GameLogic.grid);
                //Send ownPlayer
                Player ownPlayer = Server.clients[toClient].Player;
                packet.Write(ownPlayer.Name);
                if (ownPlayer.Tribe == null)
                {
                    packet.Write(Tribe.ID_NO_TRIBE);
                }
                else
                {
                    packet.Write(ownPlayer.Tribe.Id);
                }
                packet.Write(ownPlayer.Position);
                packet.Write(ownPlayer.TroopInventory);
                packet.Write(ownPlayer.Quests);
                packet.Write(ownPlayer.Avatar);

                //Send other Players
                packet.Write(GameLogic.Players.Count);
                foreach (Player player in GameLogic.Players)
                {
                    packet.Write(player.Name);
                    
                    Tribe tribe = player.Tribe;
                    if (tribe == null)
                    {
                        packet.Write(Tribe.ID_NO_TRIBE);
                    }
                    else
                    {
                        packet.Write(tribe.Id);
                    }
                    packet.Write(player.Position);
                    packet.Write(player.TroopInventory);
                    packet.Write(player.Avatar);
                }
                
                // Tribe data
                packet.Write(GameLogic.Tribes.Count);
                foreach (Tribe tribe in GameLogic.Tribes)
                {
                    packet.Write(tribe.Id);
                    packet.Write(tribe.Researches.Count);
                    foreach (Research research in tribe.Researches)
                    {
                        packet.Write(research.Code);
                    }
                }
                
                // Guild data
                packet.Write(GameLogic.Guilds.Count);
                foreach (Guild guild in GameLogic.Guilds)
                {
                    packet.Write(guild);
                }
                
                SendTCPData(toClient, packet);
            }
        }

        public static Packet SaveGameState()
        {
            Packet packet = new Packet();

            // save grid
            packet.Write(GameLogic.grid);

            // save players
            packet.Write(GameLogic.Players.Count);
            foreach (Player player in GameLogic.Players)
            {
                packet.Write(player.Name);
                Tribe tribe = player.Tribe;
                if (tribe == null)
                {
                    packet.Write(Tribe.ID_NO_TRIBE);
                    packet.Write(Guild.ID_NO_GUILD);
                }
                else
                {
                    packet.Write(tribe.Id);
                    packet.Write(tribe.Researches.Count);
                    foreach (Research research in tribe.Researches)
                    {
                        packet.Write(research.Code);
                    }

                    Guild guild = GameLogic.GetGuild(tribe.GuildId);
                    if (guild != null)
                    { 
                        packet.Write(guild);
                    }
                    else
                    {
                        packet.Write(Guild.ID_NO_GUILD);
                    }
                }
                packet.Write(player.Position);
                packet.Write(player.TroopInventory);
                packet.Write(player.Quests);
                packet.Write(player.Avatar);
            }
            return packet;
        }

        // Called when the server starts and a savegame.hex file is present (i.e. not a new game).
        public static void LoadGameState(Packet packet)
        {
            // load grid
            HexGrid hexGrid = packet.ReadHexGrid();
            GameLogic.Init(hexGrid);

            // load players
            int numberOfPlayers = packet.ReadInt();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                string playerName = packet.ReadString();
                
                int tribeId = packet.ReadByte();
                if (tribeId != Tribe.ID_NO_TRIBE)
                {
                    Tribe tribe = GameLogic.GetTribe(tribeId);
                    int numberOfResearches = packet.ReadInt();
                    for (int j = 0; j < numberOfResearches; ++j)
                    {
                        int researchCode = packet.ReadInt();
                        tribe.AddResearch(new Research(researchCode));
                    }   
                }
                
                byte guildId = packet.ReadByte();
                if (guildId != Guild.ID_NO_GUILD)
                {
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
                        Console.WriteLine($"Adding tribe {memberTribe.Id} to guild {guild.Id}");
                        if (!guild.Members.Contains(memberTribe))
                        {
                            guild.Members.Add(memberTribe);
                        }
                        memberTribe.GuildId = guildId;   // Includes FoundingMember
                    }
                }
                Console.WriteLine($"List of guilds: [{string.Join(", ", GameLogic.Guilds.Select(guild1 => guild1.Id))}]");
                
                HexCoordinates playerPos = packet.ReadHexCoordinates();
                TroopInventory troopInventory = packet.ReadTroopInventory();
                List<List<Quest>> quests = packet.ReadQuestlines();
                byte avatar = packet.ReadByte();

                GameLogic.AddPlayer(playerName, tribeId, playerPos, troopInventory, quests,avatar);
            }
            
            // TODO: for debugging only, remove these lines (Console.WriteLine("Num players/listof players...")) once finished
            Console.WriteLine($"Number of players: {GameLogic.Players.Count}");
            Console.WriteLine($"List of players: [{string.Join(", ", GameLogic.Players.Select(player => player.Name))}]");
            foreach (Guild guild in GameLogic.Guilds)
            {
                Console.WriteLine($"Guild {guild.Id}: [{string.Join(", ", guild.Members.Select(tribe => tribe.Id))}]");
            }
        }

        public static void SendStructure(HexCoordinates coordinates, Structure structure)
        {
            using (Packet packet = new Packet((int)ServerPackets.sendStructure))
            {
                packet.Write(coordinates);
                packet.Write(structure);
                SendTCPDataToAll(packet);
            }
        }

        public static void SendGameTick()
        {
            using (Packet packet = new Packet((int)ServerPackets.gameTick))
            {
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastUpgradeBuilding(HexCoordinates coordinates,Feedback feedback)
        {
            using (Packet packet = new Packet((int)ServerPackets.upgradeBuilding))
            {
                packet.Write(coordinates);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastRepairBuilding(HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.repairBuilding))
            {
                packet.Write(coordinates);
                SendTCPDataToAll(packet);
            }
        }
        public static void BroadcastSalvageBuilding(HexCoordinates coordinates)
        {
            using (Packet packet = new Packet((int)ServerPackets.salvageBuilding))
            {
                packet.Write(coordinates);
                SendTCPDataToAll(packet);
            }
        }


        public static void BroadcastApplyBuild(HexCoordinates coords, Type buildingType, Player player, Feedback feedback)
        {
            using (Packet packet = new Packet((int)ServerPackets.applyBuild))
            {
                packet.Write(coords);
                packet.Write(buildingType);
                packet.Write(player.Tribe.Id);
                packet.Write(player.Name);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastApplyBuildHQ(HexCoordinates coords)
        {
            using (Packet packet = new Packet((int)ServerPackets.applyBuildHQ))
            {
                packet.Write(coords);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastCreateGuild(byte foundingTribeId, Feedback feedback)
        {
            using (Packet packet = new Packet((int) ServerPackets.createGuild))
            {
                packet.Write(foundingTribeId);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastLeaveGuild(byte leavingTribeId, Feedback feedback)
        {
            using (Packet packet = new Packet((int) ServerPackets.leaveGuild))
            {
                packet.Write(leavingTribeId);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastJoinGuild(byte guildToJoinId, byte joiningTribeId, Feedback feedback)
        {
            using (Packet packet = new Packet((int) ServerPackets.joinGuild))
            {
                packet.Write(guildToJoinId);
                packet.Write(joiningTribeId);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastGuildDonation(byte guildId, byte tribeId, SortedDictionary<RessourceType, int> progressMap)
        {
            using (Packet packet = new Packet((int) ServerPackets.guildDonation))
            {
                packet.Write(guildId);
                packet.Write(tribeId);
                packet.Write(progressMap.Count);
                for (int i = 0; i < progressMap.Count; ++i)
                {
                    packet.Write((int)progressMap.ElementAt(i).Key);
                    packet.Write(progressMap.ElementAt(i).Value);
                }
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastDepositToGuild(byte guildId, byte tribeId, 
            SortedDictionary<RessourceType, int> currentResources, Feedback feedback)
        {
            using (Packet packet = new Packet((int) ServerPackets.depositToGuild))
            {
                packet.Write(guildId);
                packet.Write(tribeId);
                packet.Write(feedback);
                packet.Write(currentResources.Count);
                for (int i = 0; i < currentResources.Count; ++i)
                {
                    packet.Write((int)currentResources.ElementAt(i).Key);
                    packet.Write(currentResources.ElementAt(i).Value);
                }
                SendTCPDataToAll(packet);
            }
        }
        public static void BroadcastWithdrawalFromGuild(byte guildId, byte tribeId, 
            SortedDictionary<RessourceType, int> currentResources, Feedback feedback)
        {
            using (Packet packet = new Packet((int) ServerPackets.withdrawalFromGuild))
            {
                packet.Write(guildId);
                packet.Write(tribeId);
                packet.Write(feedback);
                packet.Write(currentResources.Count);
                for (int i = 0; i < currentResources.Count; ++i)
                {
                    packet.Write((int)currentResources.ElementAt(i).Key);
                    packet.Write(currentResources.ElementAt(i).Value);
                }
                SendTCPDataToAll(packet);
            }
        }
        
        public static void BroadcastBoostBaseResourceSelectionGuild(HexCoordinates coords, RessourceType type)
        {
            using (Packet packet = new Packet((int)ServerPackets.boostBaseResourceSelectionGuild))
            {
                Console.WriteLine($"Sending boost base resource selection: {type}");
                packet.Write(coords);
                packet.Write((byte)type);
                SendTCPDataToAll(packet);
            }
        }
        
        public static void BroadcastBoostAdvancedResourceSelectionGuild(HexCoordinates coords, RessourceType type)
        {
            using (Packet packet = new Packet((int)ServerPackets.boostAdvancedResourceSelectionGuild))
            {
                Console.WriteLine($"Sending boost advanced resource selection: {type}");
                packet.Write(coords);
                packet.Write((byte)type);
                SendTCPDataToAll(packet);
            }
        }
        
        public static void BroadcastBoostWeaponSelectionGuild(HexCoordinates coords, RessourceType type)
        {
            using (Packet packet = new Packet((int)ServerPackets.boostWeaponSelectionGuild))
            {
                Console.WriteLine($"Sending boost weapon selection: {type}");
                packet.Write(coords);
                packet.Write((byte)type);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastPlayer(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastPlayer))
            {
                packet.Write(player.Name);
                if (player.Tribe == null)
                {
                    packet.Write(Tribe.ID_NO_TRIBE);
                }
                else
                {
                    packet.Write(player.Tribe.Id);
                }
                packet.Write(player.Position);
                packet.Write(player.TroopInventory);
                packet.Write(player.Avatar);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastTribe(Tribe tribe)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastTribe))
            {
                packet.Write(tribe.Id);
                packet.Write(tribe.HQ.Cell.coordinates);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastMoveTroops(HexCoordinates source, HexCoordinates dest, TroopType type, int amount)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastMoveTroops))
            {
                packet.Write(source);
                packet.Write(dest);
                packet.Write((byte)type);
                packet.Write(amount);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastFight(Player player, HexCoordinates attacker, HexCoordinates defender,Feedback feedback)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastFight))
            {
                packet.Write(player.Name);
                packet.Write(attacker);
                packet.Write(defender);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastHarvest(Player player, HexCoordinates coordinates, Feedback feedback)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastHarvest))
            {
                packet.Write(player.Name);
                packet.Write(coordinates);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void broadcastCollect(Player player, HexCoordinates coordinates, Feedback feedback) 
        {
            Console.WriteLine("broadcast wird ausgeführt");
            using (Packet packet = new Packet((int)ServerPackets.broadcastCollect)) 
            {
                packet.Write(player.Name);
                packet.Write(coordinates);
                // packet.Write(feedback);  TODO: uncomment once fixed in Feedback
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastResearch(byte tribeId, int researchCode, Feedback feedback)
        {
            using (Packet packet = new Packet((int) ServerPackets.broadcastResearch))
            {
                packet.Write(tribeId);
                packet.Write(researchCode);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeAllowedRessource(HexCoordinates originCoordinates, HexCoordinates destinationCoordinates, RessourceType ressourceType, bool newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeAllowedRessource))
            {
                packet.Write(originCoordinates);
                packet.Write(destinationCoordinates);
                packet.Write((byte)ressourceType);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeTroopRecipeOfBarracks(HexCoordinates barracks, TroopType troopType)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeTroopRecipeOfBarracks))
            {
                packet.Write(barracks);
                packet.Write((byte)troopType);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeStrategyOfProtectedBuilding(HexCoordinates coordinates, int oldIndex, int newIndex)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyOfProtectedBuilding))
            {
                packet.Write(coordinates);
                packet.Write(oldIndex);
                packet.Write(newIndex);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeStrategyOfPlayer(string playerName, int oldIndex, int newIndex)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyOfName))
            {
                packet.Write(playerName);
                packet.Write(oldIndex);
                packet.Write(newIndex);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeStrategyActivePlayer(string playerName, TroopType type, bool newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyActivePlayer))
            {
                packet.Write(playerName);
                packet.Write((byte)type);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeStrategyActiveBuilding(HexCoordinates coordinates, TroopType type, bool newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeStrategyActiveBuilding))
            {
                packet.Write(coordinates);
                packet.Write((byte)type);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastMoveRessources(HexCoordinates originCoordinates, HexCoordinates destinationCoordinates, RessourceType type, int amount)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastMoveRessources))
            {
                packet.Write(originCoordinates);
                packet.Write(destinationCoordinates);
                packet.Write((byte)type);
                packet.Write(amount);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastChangeRessourceLimit(HexCoordinates coordinates, RessourceType type, int newValue)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChangeRessourceLimit))
            {
                packet.Write(coordinates);
                packet.Write((byte)type);
                packet.Write(newValue);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastRecipeChange(HexCoordinates coords, RessourceType ressource)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastRecipeChange))
            {
                packet.Write(coords);
                packet.Write((byte)ressource);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastUpdateMarket(HexCoordinates coords, RessourceType type, bool isInput)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastUpdateMarketRessource))
            {
                packet.Write(coords);
                packet.Write((byte)type);
                packet.Write(isInput);
                SendTCPDataToAll(packet);
            }
        }

        public static void BroadcastDestroyBuilding(HexCoordinates coords, Feedback feedback)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastDestroyBuilding))
            {
                packet.Write(coords);
                packet.Write(feedback);
                SendTCPDataToAll(packet);
            }
        }
        public static void BroadcastChatMessage(String message, byte tribeId, byte guildId, string playerName, int chatType)
        {
            using (Packet packet = new Packet((int)ServerPackets.broadcastChatMessage))
            {
                packet.Write(message);
                packet.Write(tribeId);
                packet.Write(guildId);
                packet.Write(playerName);
                packet.Write(chatType);
                SendTCPDataToAll(packet);
                //foreach (KeyValuePair<int, Client> entry in Server.clients)
                //{
                //    if (entry.Value.Player?.Tribe == tribe)
                //    {
                //        SendTCPData(entry.Key, packet);
                //    }
                //}
            }
        }

        public static void BroadcastPrioChange(Tribe tribe, int prio, bool increase)
        {
            using (Packet packet = new Packet((int)ServerPackets.prioChange))
            {
                packet.Write(tribe.Id);
                packet.Write(prio);
                packet.Write(increase);
                SendTCPDataToAll(packet);
            }
        }
        public static void BroadcastWeather(HexCoordinates coordinates, string weather)
        {
            using (Packet packet = new Packet((int) ServerPackets.broadcastWeather))
            {
                packet.Write(coordinates);
                packet.Write(weather);
                SendTCPDataToAll(packet);
            }
        }

        public static void sendQuests(uint[] allQuests, uint[] successfulQuests, Player player)
        {
            int toClient = getClientId(player);
            using (Packet packet = new Packet((int)ServerPackets.sendQuests))
            {
                if (toClient >= 0)
                {
                    packet.Write(allQuests);
                    packet.Write(successfulQuests);
                    packet.Write(player.Name);
                    SendTCPData(toClient, packet);
                }
            }
        }

        public static int getClientId(Player player)
        {
            foreach (KeyValuePair<int, Client> entry in Server.clients)
            {
                if (entry.Value.Player == player)
                {
                    return entry.Key;
                }
            }
            Console.WriteLine("Player not found.");
            return -1;
        }
    }
}
