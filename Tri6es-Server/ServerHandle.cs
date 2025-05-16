using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Communication;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Structures;
using Shared.Game;

namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeRecieved(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}");

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
            }
            Player clientPlayer = GameLogic.GetPlayer(username);
            if (clientPlayer != null)
            {
                Console.WriteLine("not null");
                foreach (Client client in Server.clients.Values)
                {
                    if (client.Player == clientPlayer)
                    {
                        Console.WriteLine("A Player has connected with a name which is already assigned to an active player. Assuming that this is due to a network change the server tries to sync the client with the lost packages!");

                        int receivedClientPackages = packet.ReadInt();
                        foreach (KeyValuePair<int, Client> kvp in Server.clients)
                        {
                            if (kvp.Value.Player == clientPlayer && kvp.Value != client)
                            {
                                int missingPackages = kvp.Value.tcp.sentPackages - receivedClientPackages;
                                if (missingPackages > PacketBuffer.size)
                                {
                                    Console.WriteLine("Number of lost packages were too high. Full Reconnect for client necessary!");
                                    ServerSend.InitGameLogic(fromClient);
                                }
                                else
                                {
                                    List<byte[]> lostPackages = PacketBuffer.GetLostPackets(missingPackages);
                                    Server.clients[fromClient].tcp.SendData(lostPackages);
                                }
                                Server.clients[fromClient].tcp.sentPackages = 0;
                                // Server.clients[fromClient].tcp.sentPackages = kvp.Value.tcp.sentPackages;
                                Server.clients[kvp.Key].Disconnect();
                                break;
                            }
                        }
                    }
                }
                Server.clients[fromClient].Player = clientPlayer;
            }
            else
            {
                Console.WriteLine("null");
                Server.clients[fromClient].Player = GameLogic.AddPlayer(username, null);
                clientPlayer = GameLogic.GetPlayer(username);
            }

            //Stop Timer
            Server.StopPingTest(fromClient);
            ServerSend.InitGameLogic(fromClient);
            HexCoordinates coordinates = clientPlayer.Position;
            string weather = GameLogic.UpdateWeather(coordinates);
            ServerSend.BroadcastWeather(coordinates, weather);
        }

        public static void HandleMapRequest(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            ServerSend.InitGameLogic(fromClient);
        }

        public static void HandleBuildHQ(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            Headquarter hq = new Headquarter();

            if (GameLogic.VerifyBuildHQ(coordinates, hq, player))
            {
                Tribe tribe = GameLogic.ApplyBuildHQ(coordinates, hq);
                player.Tribe = tribe;
                //player will take automatically the first avatar when hq is built for the first
                player.Avatar = 0;
                ServerSend.BroadcastApplyBuildHQ(coordinates);
                ServerSend.BroadcastPlayer(player);
                Console.WriteLine("Player: " + player.Name + " successfully placed a HQ.");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + " failed to build HQ");
            }
            Server.SaveGame();
        }

        public static void HandlePlaceBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            Type type = packet.ReadType();

            Building building = (Building)Activator.CreateInstance(type);

            Feedback feedback = new Feedback(Feedback.FeedbackStyle.build);


            if (GameLogic.VerifyBuild(coordinates, type, player, ref feedback))
            {
                GameLogic.ApplyBuild(coordinates, type, player.Tribe);

                ServerSend.BroadcastApplyBuild(coordinates, type, player, feedback);
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " successfully placed a " + building.GetFriendlyName() + ".");
            }
            else
            {
                ServerSend.BroadcastApplyBuild(coordinates, type, player, feedback);
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " failed to build " + building.GetFriendlyName() + ".");
            }
            Server.SaveGame();
        }

        public static void HandleUpgradeBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            Feedback feedback = new Feedback(Feedback.FeedbackStyle.upgrade);

            HexCoordinates coordinates = packet.ReadHexCoordinates();

            if (GameLogic.VerifyUpgrade(coordinates, player, ref feedback))
            {
                GameLogic.ApplyUpgrade(coordinates, player.Tribe);
                ServerSend.BroadcastUpgradeBuilding(coordinates,feedback);
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " successfully upgraded a building at " + coordinates.ToString() + ".");
            }
            else
            {
                ServerSend.BroadcastUpgradeBuilding(coordinates, feedback);
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " failed upgrade building at " + coordinates.ToString());
            }
            Server.SaveGame();
        }

        public static void HandleRepairBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            if (GameLogic.VerifyRepair(coordinates, player))
            {
                GameLogic.ApplyRepairBuilding(coordinates, player.Tribe);
                ServerSend.BroadcastRepairBuilding(coordinates);
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " successfully repaired a building at " + coordinates.ToString() + ".");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " failed repair building at " + coordinates.ToString() + "");
            }
            Server.SaveGame();
        }
        public static void HandleSalvageBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();
            if (GameLogic.verifySalvage(coordinates, player))
            {
                GameLogic.ApplySalvage(coordinates, player.Tribe);
                ServerSend.BroadcastSalvageBuilding(coordinates);
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " successfully salvaged a building at " + coordinates.ToString() + ".");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + " of tribe " + player.Tribe.Id.ToString() + " failed repair building at " + coordinates.ToString() + "");
            }
            Server.SaveGame();
        }

        public static void HandlePositionUpdate(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();
            HexCoordinates prevPos = player.Position;
            player.Position = coordinates;
            ServerSend.BroadcastPlayer(player);

            // TODO new player position received - check for local weather
            if (prevPos != coordinates)
            {
                string weather = GameLogic.UpdateWeather(coordinates);
                ServerSend.BroadcastWeather(coordinates, weather);
            }

        }

        public static void HandleJoinTribe(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();
            if (GameLogic.PlayerInRange(coordinates, player) && player.Tribe == null)
            {
                HexCell cell = GameLogic.grid.GetCell(coordinates);
                if (cell != null)
                {
                    Structure hq = cell.Structure;
                    if (hq is Headquarter)
                    {
                        player.Tribe = GameLogic.GetTribe(((Headquarter)hq).Tribe);
                        player.Tribe.size += 1; // TODO: check intention = +1 or += 1
                        player.Avatar = packet.ReadByte();

                        ServerSend.BroadcastPlayer(player);
                        Console.WriteLine("Player: " + player.Name + " successfully joined the tribe " + player.Tribe.Id.ToString() + "with the avatarID of :"+$"{player.Avatar}" +".");
                        return;
                    }
                }
            }
            Console.WriteLine("Player: " + player.Name + " failed to join the tribe " + ".");
        }
        public static void HandleLeaveTribe(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            Player player = Server.clients[fromClient].Player;
            int id = player.Tribe.Id;
            player.Tribe.size -= 1; //TODO handle when size becomes 0 | also, is this thread safe?
            player.Tribe = null;
            player.Avatar = (byte)255;
            ServerSend.BroadcastPlayer(player);
            Console.WriteLine("Player: " + player.Name + "successfully left the tribe " + id + ".");
        }

        public static void HandleCreateGuild(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();

            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIdCheck}\"!");
            }

            // TODO: might have to check whether player/tribe member has rights to create a guild
            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();

            Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage);
            Tribe foundingTribe = player.Tribe;
            
            if (GameLogic.PlayerInRange(coordinates, player) && player.Tribe.HasNoGuild())
            {
                feedback.successfull = true;
                feedback.message = "Successfully created guild.";
                
                GameLogic.CreateGuild(foundingTribe);
                ServerSend.BroadcastCreateGuild(foundingTribe.Id, feedback);
                ServerSend.BroadcastPlayer(player);
                Console.WriteLine($"Tribe {player.Tribe.Id} created a new guild.");
            }
            else
            {
                // Consider: player.Tribe should always be != null, otherwise couldn't have pressed "Create Guild"
                Console.WriteLine($"Tribe {player.Tribe.Id} failed to create a new guild.");
                
                feedback.successfull = true;
                feedback.message = "Failed to create guild.";
                ServerSend.BroadcastCreateGuild(foundingTribe.Id, feedback);
            }

            Server.SaveGame();
        }

        public static void HandleLeaveGuild(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();

            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIdCheck}\"!");
            }

            // TODO: might have to check whether player can already leave
            Player player = Server.clients[fromClient].Player;
            byte guildId = packet.ReadByte();
            HexCoordinates coordinates = packet.ReadHexCoordinates();

            Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage);
            Tribe leavingTribe = player.Tribe;
            
            if (GameLogic.PlayerInRange(coordinates, player) && guildId != Guild.ID_NO_GUILD)
            {
                feedback.successfull = true;
                feedback.message = "Successfully left guild.";
                
                GameLogic.LeaveGuild(leavingTribe);
                GameLogic.ChangeGuildHouseLevel(leavingTribe, 1);
                
                // Important: 1. BroadcastPlayer, 2. BroadcastLeaveGuild
                // Else, particles won't play if GuildHouse level changes...
                // The cause is HexMeshGrid::UpdateActiveChunks() which is called in ClientHandle::ReceivePlayer().
                ServerSend.BroadcastPlayer(player);
                ServerSend.BroadcastLeaveGuild(leavingTribe.Id, feedback);
                
                Console.WriteLine($"Tribe {player.Tribe.Id} left guild {guildId}.");
            }
            else
            {
                // Consider: player.Tribe should always be != null, otherwise couldn't have pressed "Leave Guild"
                Console.WriteLine($"Tribe {player.Tribe.Id} failed to leave guild {player.Tribe.GuildId}.");
                
                feedback.successfull = false;
                feedback.message = "Failed to leave guild.";
                ServerSend.BroadcastLeaveGuild(leavingTribe.Id, feedback);
            }
            
            Server.SaveGame();
        }

        public static void HandleJoinGuild(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();

            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIdCheck}\"!");
            }
            
            // TODO: check for approval (i.e. guild leaders should confirm before new tribe can join)
            Player player = Server.clients[fromClient].Player;
            byte guildToJoinId = packet.ReadByte();
            HexCoordinates coordinates = packet.ReadHexCoordinates();

            Guild guild = GameLogic.GetGuild(guildToJoinId);
            Tribe foundingMember = guild.FoundingMember;
            float distanceHQs = HexCoordinates.calcDistance(player.Tribe.HQ.Cell.coordinates, foundingMember.HQ.Cell.coordinates);

            int maxDistanceHq = 30; // TODO: put this constant somewhere or make it scale with Guild's level?
            
            bool isPlayerInRange = GameLogic.PlayerInRange(coordinates, player);
            bool playerTribeHasNoGuild = player.Tribe.HasNoGuild();
            bool otherTribeHasGuild = guildToJoinId != Guild.ID_NO_GUILD;
            bool isHqCloseEnoughToFounder = distanceHQs < maxDistanceHq; 
            bool isGuildFull = guild.IsFull();

            Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage);
            Tribe joiningTribe = player.Tribe;
            
            if (isPlayerInRange && playerTribeHasNoGuild && otherTribeHasGuild && isHqCloseEnoughToFounder && !isGuildFull)
            {
                feedback.successfull = true;
                feedback.message = "Successfully joined guild.";
                
                GameLogic.JoinGuild(guildToJoinId, joiningTribe);
                GameLogic.ChangeGuildHouseLevel(joiningTribe, (byte)guild.Level);
                
                // Important: 1. BroadcastPlayer, 2. BroadcastJoinGuild
                // Else, particles won't play if GuildHouse level changes...
                // The cause is HexMeshGrid::UpdateActiveChunks() which is called in ClientHandle::ReceivePlayer().
                ServerSend.BroadcastPlayer(player);
                ServerSend.BroadcastJoinGuild(guildToJoinId, joiningTribe.Id, feedback);
                
                Console.WriteLine($"Tribe {joiningTribe.Id} joined guild {guildToJoinId}.");
            }
            else
            {
                feedback.successfull = false;
                feedback.message = "Failed to join guild.";
                
                Console.WriteLine($"Tribe {player.Tribe.Id} failed to join guild {guildToJoinId}.");

                if (!isHqCloseEnoughToFounder)
                {
                    Console.WriteLine($"Distance between HQs is {distanceHQs}.");
                    feedback.message += $"\nDistance between HQs is {distanceHQs}. Max is {maxDistanceHq}.";
                }

                if (isGuildFull)
                {
                    Console.WriteLine($"Guild is already full: {guild.Members.Count}/{guild.GetMaxMembers()}");
                    feedback.message += $"\nGuild is already full.";
                }
                
                ServerSend.BroadcastJoinGuild(guildToJoinId, joiningTribe.Id, feedback);
            }
            
            Server.SaveGame();
        }

        public static void HandleGuildDonation(int fromClient, Packet packet)
        {
            // == Get the data from the packet ========================================================================
            
            int clientIdCheck = packet.ReadInt();
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIdCheck}\"!");
            }
            
            Dictionary<RessourceType, int> donationMap = new Dictionary<RessourceType, int>();

            byte guildId = packet.ReadByte();   // Should always be != ID_NO_GUILD
            byte tribeId = packet.ReadByte();   // != ID_NO_TRIBE
            int mapSize = packet.ReadInt();
            for (int i = 0; i < mapSize; ++i)
            {
                RessourceType key = (RessourceType) packet.ReadInt();
                int value = packet.ReadInt();
                donationMap[key] = value;
                Console.WriteLine($"HandleGuildDonation: {key}, {value}");
            }
            
            // == Apply game logic ====================================================================================

            Guild guild = GameLogic.GetGuild(guildId);
            Tribe tribe = GameLogic.GetTribe(tribeId);
            
            SortedDictionary<RessourceType, int> progressMap = guild.GetCurrentProgressMap();
            Dictionary<RessourceType, int> actualDonationMap = new Dictionary<RessourceType, int>();

            foreach (KeyValuePair<RessourceType, int> pair in donationMap)
            {
                RessourceType key = pair.Key;
                
                // E.g. donated 9, but progress only 4 remaining -> should not donate more than 4
                int remainingAmount = progressMap[key];
                int attemptedDonation = donationMap[key];
                actualDonationMap[key] = Math.Min(remainingAmount, attemptedDonation);
            }

            Console.WriteLine(string.Join(", ", progressMap.Values));
            GameLogic.DonateToGuild(guild, tribe, actualDonationMap);
            Console.WriteLine(string.Join(", ", progressMap.Values));

            // Regardless of progress reached or not, must send back current progress
            ServerSend.BroadcastGuildDonation(guildId, tribeId, progressMap);
            
            if (guild.IsProgressReached())
            {
                // Level up, upgrade GuildHouse of all guild members
                guild.LevelUp();
                foreach (Tribe guildMember in guild.Members)
                {
                    GuildHouse guildHouse = guildMember.GetGuildHouse();
                    if (guildHouse != null && guildHouse.IsUpgradable())
                    {
                        Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage)
                        {
                            message = "Guild leveled up. GuildHouse upgraded.",
                            successfull = true,
                        };

                        HexCoordinates guildHouseCoords = guildHouse.Cell.coordinates;
                        GameLogic.ApplyUpgrade(guildHouseCoords, guildMember);
                        ServerSend.BroadcastUpgradeBuilding(guildHouseCoords, feedback);
                    }
                }
            }
            
            Server.SaveGame();
        }

        public static void HandleDepositToGuild(int fromClient, Packet packet)
        {
            // == Get the data from the packet ========================================================================
            
            int clientIdCheck = packet.ReadInt();
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIdCheck}\"!");
            }
            
            Dictionary<RessourceType, int> amountMap = new Dictionary<RessourceType, int>();

            byte guildId = packet.ReadByte();   // Should always be != ID_NO_GUILD
            byte tribeId = packet.ReadByte();   // != ID_NO_TRIBE
            int mapSize = packet.ReadInt();
            for (int i = 0; i < mapSize; ++i)
            {
                RessourceType key = (RessourceType) packet.ReadInt();
                int value = packet.ReadInt();
                amountMap[key] = value;
                Console.WriteLine($"HandleDepositToGuild: {key}, {value}");
            }
            
            // == Apply game logic ====================================================================================

            Guild guild = GameLogic.GetGuild(guildId);
            Tribe tribe = GameLogic.GetTribe(tribeId);

            SortedDictionary<RessourceType, int> currentResources = guild.Inventory.Resources;
            Dictionary<RessourceType, int> actualDepositMap = new Dictionary<RessourceType, int>();

            foreach (KeyValuePair<RessourceType, int> pair in currentResources)
            {
                RessourceType type = pair.Key;

                // E.g. attempt to deposit 10 wood, but only 5 available in tribe inventory.
                int availableAmount = tribe.tribeInventory.GetRessourceAmount(type);
                int attemptedAmount = amountMap[type];
                actualDepositMap[type] = Math.Min(availableAmount, attemptedAmount);
            }
            
            Console.WriteLine(string.Join(", ", currentResources.Values));
            guild.Inventory.DepositResources(tribe, actualDepositMap);
            Console.WriteLine(string.Join(", ", currentResources.Values));
            
            // == Construct feedback ==================================================================================
            
            Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage);
            feedback.successfull = true;
            feedback.message = "Deposited ";
            foreach (KeyValuePair<RessourceType, int> pair in actualDepositMap)
            {
                if (pair.Value != 0)
                {
                    feedback.message += $"{pair.Value} {pair.Key}, ";
                }
            }
            // Remove the last comma
            feedback.message = feedback.message.Remove(feedback.message.Length - 2);
            
            // == Send the new inventory status to the client =========================================================
            ServerSend.BroadcastDepositToGuild(guild.Id, tribe.Id, currentResources, feedback);
            
            Server.SaveGame();
        }
        
        public static void HandleWithdrawalFromGuild(int fromClient, Packet packet)
        {
            // == Get the data from the packet ========================================================================
            
            int clientIdCheck = packet.ReadInt();
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIdCheck}\"!");
            }
            
            Dictionary<RessourceType, int> amountMap = new Dictionary<RessourceType, int>();

            byte guildId = packet.ReadByte();   // Should always be != ID_NO_GUILD
            byte tribeId = packet.ReadByte();   // != ID_NO_TRIBE
            int mapSize = packet.ReadInt();
            for (int i = 0; i < mapSize; ++i)
            {
                RessourceType key = (RessourceType) packet.ReadInt();
                int value = packet.ReadInt();
                amountMap[key] = value;
                Console.WriteLine($"HandleWithdrawalFromGuild: {key}, {value}");
            }
            
            // == Apply game logic ====================================================================================

            Guild guild = GameLogic.GetGuild(guildId);
            Tribe tribe = GameLogic.GetTribe(tribeId);

            SortedDictionary<RessourceType, int> currentResources = guild.Inventory.Resources;
            Dictionary<RessourceType, int> actualWithdrawalMap = new Dictionary<RessourceType, int>();

            foreach (KeyValuePair<RessourceType, int> pair in currentResources)
            {
                RessourceType type = pair.Key;

                // E.g. attempt to withdraw 10 wood, but only 5 available in guild inventory.
                // Also make sure there is enough space in tribe inventory to store the resources.
                int availableAmount = guild.Inventory.GetResourceAmount(type);
                int availableSpace = tribe.tribeInventory.GetLimit(type) - tribe.tribeInventory.GetRessourceAmount(type);
                int attemptedAmount = amountMap[type];
                
                Console.WriteLine($"Available space: {availableSpace}");
                
                // Inner min: makes sure we don't withdraw more than available in guild inventory.
                // Outer min: makes sure we don't withdraw more than we can store in tribe inventory.
                actualWithdrawalMap[type] = Math.Min(availableSpace, Math.Min(availableAmount, attemptedAmount));
            }
            
            Console.WriteLine(string.Join(", ", currentResources.Values));
            guild.Inventory.WithdrawResources(tribe, actualWithdrawalMap);
            Console.WriteLine(string.Join(", ", currentResources.Values));
            
            // == Construct feedback ==================================================================================

            Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage);
            feedback.successfull = true;
            
            bool hasWithdrawnAnything = actualWithdrawalMap.Values.Any(amount => amount > 0);
            if (hasWithdrawnAnything)
            {
                feedback.message = "Withdrawn ";
                foreach (KeyValuePair<RessourceType, int> pair in actualWithdrawalMap)
                {
                    if (pair.Value != 0)
                    {
                        feedback.message += $"{pair.Value} {pair.Key}, ";
                    }
                    
                    Console.WriteLine($"... {pair.Key} - {pair.Value}");
                }
                
                // Remove the last comma
                feedback.message = feedback.message.Remove(feedback.message.Length - 2);
            }
            else
            {
                feedback.message = "Nothing withdrawn. Check available space in inventory.";
            }
            
            // == Send the new inventory status to the client =========================================================
            ServerSend.BroadcastWithdrawalFromGuild(guild.Id, tribe.Id, currentResources, feedback);
            
            Server.SaveGame();
        }

        public static void HandleBoostBaseResourceSelectionGuild(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            
            HexCoordinates coords = packet.ReadHexCoordinates();
            RessourceType resourceType = (RessourceType)packet.ReadByte();
            if (GameLogic.PlayerInRange(coords, Server.clients[fromClient].Player))
            {
                if (GameLogic.UpdateBoostBaseResourceSelectionGuild(coords, resourceType))
                {
                    ServerSend.BroadcastBoostBaseResourceSelectionGuild(coords, resourceType);
                }
            }
        }

        public static void HandleBoostAdvancedResourceSelectionGuild(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            
            HexCoordinates coords = packet.ReadHexCoordinates();
            RessourceType resourceType = (RessourceType)packet.ReadByte();
            if (GameLogic.PlayerInRange(coords, Server.clients[fromClient].Player))
            {
                if (GameLogic.UpdateBoostAdvancedResourceSelectionGuild(coords, resourceType))
                {
                    ServerSend.BroadcastBoostAdvancedResourceSelectionGuild(coords, resourceType);
                }
            }
        }

        public static void HandleBoostWeaponSelectionGuild(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            
            HexCoordinates coords = packet.ReadHexCoordinates();
            RessourceType resourceType = (RessourceType)packet.ReadByte();
            if (GameLogic.PlayerInRange(coords, Server.clients[fromClient].Player))
            {
                if (GameLogic.UpdateBoostWeaponSelectionGuild(coords, resourceType))
                {
                    ServerSend.BroadcastBoostWeaponSelectionGuild(coords, resourceType);
                }
            }
        }

        public static void HandleMoveTroops(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates source = packet.ReadHexCoordinates();
            HexCoordinates dest = packet.ReadHexCoordinates();
            TroopType troopType = (TroopType)packet.ReadByte();
            int amount = packet.ReadInt();
            Player player = Server.clients[fromClient].Player;

            if (GameLogic.MoveTroops(source, dest, troopType, amount))
            {
                ServerSend.BroadcastMoveTroops(source, dest, troopType, amount);
                Console.WriteLine("Player: " + player.Name + "of tribe " + player.Tribe.Id.ToString() + " successfully exchanged " + amount.ToString() + troopType.ToString() + " with a building at" + dest.ToString() + ".");
            }
            else
            {
                Console.WriteLine("Player: " + player.Name + "of tribe " + player.Tribe.Id.ToString() + " failed to exchange " + amount.ToString() + troopType.ToString() + " with building at " + dest.ToString() + ".");
            }
        }

        public static void HandleFight(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates attacker = packet.ReadHexCoordinates();
            HexCoordinates defender = packet.ReadHexCoordinates();

            Feedback feedback = new Feedback(Feedback.FeedbackStyle.battle);

            if (GameLogic.PlayerInRange(defender, player))
            {
                GameLogic.Fight(player, attacker, defender, ref feedback);
                ServerSend.BroadcastFight(player, attacker, defender,feedback);
                Console.WriteLine($"Player: {player.Name} of tribe {player.Tribe.Id} successfully fought a building at {defender} with troops from {attacker}.");
            }
            else
            {
                Console.WriteLine($"Player: {player.Name} of tribe {player.Tribe.Id} failed to fight a building at {defender} with troops from {attacker}.");
            }
        }

        public static void HandleHarvest(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();

            if (player.Tribe != null)
            {
                Feedback feedback = new Feedback(Feedback.FeedbackStyle.harvest);


                if (GameLogic.Harvest(player.Name, coordinates, ref feedback) != 0)
                {
                    ServerSend.BroadcastHarvest(player, coordinates, feedback);
                    Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " successfully harvested ressource at " + coordinates.ToString() + ".");

                    return;
                }
                else
                {
                    ServerSend.BroadcastHarvest(player, coordinates, feedback);
                }
                Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " failed to harvest ressource at " + coordinates.ToString() + ".");
            }
        }

        public static void HandleCollect(int fromClient, Packet packet) 
        {

            Console.WriteLine("Server.Handle Collect wird ausgef�hrt");
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            Player player = Server.clients[fromClient].Player;
            HexCoordinates coordinates = packet.ReadHexCoordinates();
            Feedback feedback = new Feedback(Feedback.FeedbackStyle.collect);

            if (player.Tribe != null && GameLogic.Collect(player.Name, coordinates, ref feedback)) 
            {
                ServerSend.broadcastCollect(player, coordinates, feedback);
            }
        }

        public static void HandleResearch(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            
            int researchCode = packet.ReadInt();
            Research research = new Research(researchCode);
            
            Player player = Server.clients[fromClient].Player;
            Tribe tribe = player.Tribe;
            
            Feedback feedback = new Feedback(Feedback.FeedbackStyle.plainMessage);
            if (!player.Tribe.tribeInventory.RecipeApplicable(research.Costs, ref feedback))
            {
                feedback.message += $"to research {research.Name}.";
                feedback.successfull = false;
            }
            else
            {
                feedback.message = $"Successfully researched {research.Name}";
                feedback.successfull = true;
                
                GameLogic.ApplyResearch(tribe, research);
            }
            
            ServerSend.BroadcastResearch(tribe.Id, researchCode, feedback);
            Server.SaveGame();
        }

        public static void HandleChangeAllowedRessource(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates originCoordinates = packet.ReadHexCoordinates();

            if (GameLogic.PlayerInRange(originCoordinates, player))
            {
                HexCoordinates destinationCoordinates = packet.ReadHexCoordinates();
                RessourceType ressourceType = (RessourceType)packet.ReadByte();
                bool newValue = packet.ReadBool();
                if (GameLogic.ChangeAllowedRessource(originCoordinates, destinationCoordinates, ressourceType, newValue))
                {
                    Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " changed allowed Ressource: " + ressourceType.ToString() + " at " + originCoordinates.ToString() + ".");
                    ServerSend.BroadcastChangeAllowedRessource(originCoordinates, destinationCoordinates, ressourceType, newValue);
                    return;
                }
            }
            Console.WriteLine("Player: " + player.Name + "of tribe" + player.Tribe.Id.ToString() + " failed to change allowed Ressource at " + originCoordinates.ToString() + ".");
        }

        public static void HandleChangeTroopRecipeOfBarracks(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            HexCoordinates barracks = packet.ReadHexCoordinates();
            TroopType troopType = (TroopType)packet.ReadByte();

            if (GameLogic.PlayerInRange(barracks, player))
            {
                if (GameLogic.ChangeTroopRecipeOfBarracks(barracks, troopType))
                    ServerSend.BroadcastChangeTroopRecipeOfBarracks(barracks, troopType);
            }
        }

        public static void HandleReconnect(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }


        }

        public static void HandleChangeStrategy(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            bool usePlayerTroopInventory = packet.ReadBool();
            HexCoordinates coords = packet.ReadHexCoordinates();
            int oldIndex = packet.ReadInt();
            int newIndex = packet.ReadInt();
            Player player = Server.clients[fromClient].Player;

            if (usePlayerTroopInventory)
            {
                if (GameLogic.ChangeStrategyOfPlayer(player.Name, oldIndex, newIndex))
                    ServerSend.BroadcastChangeStrategyOfPlayer(player.Name, oldIndex, newIndex);
            }
            else
            {
                if (GameLogic.PlayerInRange(coords, player))
                {
                    if (GameLogic.ChangeStrategyOfProtectedBuilding(coords, oldIndex, newIndex))
                        ServerSend.BroadcastChangeStrategyOfProtectedBuilding(coords, oldIndex, newIndex);
                }
            }
        }

        public static void HandleChangeActiveOfStrategyPlayer(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            TroopType type = (TroopType)packet.ReadByte();
            bool newValue = packet.ReadBool();

            Player player = Server.clients[fromClient].Player;

            if (GameLogic.ChangeActiveStrategyOfPlayer(player.Name, type, newValue))
                ServerSend.BroadcastChangeStrategyActivePlayer(player.Name, type, newValue);
        }

        public static void HandleChangeActiveOfStrategyBuilding(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates coordinates = packet.ReadHexCoordinates();
            TroopType type = (TroopType)packet.ReadByte();
            bool newValue = packet.ReadBool();

            Player player = Server.clients[fromClient].Player;

            if (GameLogic.PlayerInRange(coordinates, player))
            {
                if (GameLogic.ChangeActiveStrategyOfBuilding(coordinates, type, newValue))
                    ServerSend.BroadcastChangeStrategyActiveBuilding(coordinates, type, newValue);
            }
        }

        public static void HandleMoveRessource(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates originCoordinates = packet.ReadHexCoordinates();
            HexCoordinates destinationCoordinates = packet.ReadHexCoordinates();
            RessourceType ressourceType = (RessourceType)packet.ReadByte();
            int amount = packet.ReadInt();
            Player player = Server.clients[fromClient].Player;

            if (GameLogic.PlayerInRange(originCoordinates, player) || GameLogic.PlayerInRange(destinationCoordinates, player))
            {
                if (GameLogic.MoveRessources(originCoordinates, destinationCoordinates, ressourceType, amount))
                    ServerSend.BroadcastMoveRessources(originCoordinates, destinationCoordinates, ressourceType, amount);
            }
        }

        public static void HandleChangeRessourceLimit(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates buildingCoordinates = packet.ReadHexCoordinates();
            RessourceType ressourceType = (RessourceType)packet.ReadByte();
            int newValue = packet.ReadInt();
            Player player = Server.clients[fromClient].Player;

            if (GameLogic.PlayerInRange(buildingCoordinates, player))
            {
                if (GameLogic.UpdateRessourceLimit(buildingCoordinates, ressourceType, newValue))
                    ServerSend.BroadcastChangeRessourceLimit(buildingCoordinates, ressourceType, newValue);
            }
        }

        public static void HandleRecipeChange(int fromClient, Packet packet)
        {
            // TODO: move id check to own method
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates coords = packet.ReadHexCoordinates();
            RessourceType ressource = (RessourceType)packet.ReadByte();

            if (GameLogic.PlayerInRange(coords, Server.clients[fromClient].Player))
            {
                if (GameLogic.UpdateRefineryRecipe(coords, ressource))
                {
                    ServerSend.BroadcastRecipeChange(coords, ressource);
                }
            }
        }

        public static void HandleUpdateMarketRessource(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            HexCoordinates coords = packet.ReadHexCoordinates();
            RessourceType ressourceType = (RessourceType)packet.ReadByte();
            bool isInput = packet.ReadBool();

            if (GameLogic.PlayerInRange(coords, Server.clients[fromClient].Player))
            {
                if (GameLogic.UpdateMarketRessource(coords, ressourceType, isInput))
                {
                    ServerSend.BroadcastUpdateMarket(coords, ressourceType, isInput);
                }
            }
        }

        public static void HandleDestroyBuilding(int fromClient, Packet packet)
        {
            Feedback feedback = new Feedback(Feedback.FeedbackStyle.destroy);

            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }
            HexCoordinates coords = packet.ReadHexCoordinates();
            Player player = Server.clients[fromClient].Player;
            if (GameLogic.PlayerInRange(coords, player))
            {
                if (GameLogic.DestroyStructure(coords, ref feedback))
                {
                    ServerSend.BroadcastDestroyBuilding(coords, feedback);
                }
            }
            Server.SaveGame();
        }
        public static void HandleRequestChatHistory(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;

            

        }
        public static void HandleSendChatMessage(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            String playerName = player.Name;
            String message = packet.ReadString();
            int chatType = packet.ReadInt();
            ServerSend.BroadcastChatMessage(message, player.Tribe.Id, player.Tribe.GuildId, playerName, chatType);
        }
        public static void HandleMovementQuestProgress(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            int distance = packet.ReadInt();
            List<List<uint>> combinedList = new List<List<uint>>();
            combinedList = Quest.addMovementQuestProgress(player, distance);
            ServerSend.sendQuests(combinedList[0].ToArray(), combinedList[1].ToArray(), player);
            Server.SaveGame();
        }
        public static void HandleBuildingQuestProgress(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            List<List<uint>> combinedList = new List<List<uint>>();
            Type buildingType = packet.ReadType();
            combinedList = Quest.addBuildingQuestProgress(player, buildingType);
            ServerSend.sendQuests(combinedList[0].ToArray(), combinedList[1].ToArray(), player);
            Server.SaveGame();
        }
        public static void HandleRessourceQuestProgress(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            List<List<uint>> combinedList = new List<List<uint>>();
            HexCoordinates coordinates = packet.ReadHexCoordinates();
            combinedList = Quest.addRessourceQuestProgress(player, coordinates);

            if (combinedList == null) return;

            ServerSend.sendQuests(combinedList[0].ToArray(), combinedList[1].ToArray(), player);
            Server.SaveGame();
        }
        public static void HandlePrioChange(int fromClient, Packet packet)
        {
            int clientIDCheck = packet.ReadInt();

            if (fromClient != clientIDCheck)
            {
                Console.WriteLine($"Player with ID: \"{fromClient}\" has assumed the wrong client ID: \"{clientIDCheck}\"!");
            }

            Player player = Server.clients[fromClient].Player;
            int prio = packet.ReadInt();
            bool increase = packet.ReadBool();
            bool success = GameLogic.PrioChange(player.Tribe, prio, increase);
            if (success) ServerSend.BroadcastPrioChange(player.Tribe, prio, increase);
        }
    }
}
