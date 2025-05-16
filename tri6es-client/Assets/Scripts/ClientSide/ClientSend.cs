using UnityEngine;
using Shared.Communication;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSend : MonoBehaviour
{
    private static void sendTCPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    #region Packets
    public static void WelcomeRecieved()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myid);
            packet.Write(PlayerPrefs.GetString("PlayerName"));

            if (Client.instance.receivedPackages > 0)
            {
                packet.Write(Client.instance.receivedPackages);
                Client.instance.receivedPackages = 0;
            }

            sendTCPData(packet);
        }
    }

    public static void RequestHexGrid()
    {
        using (Packet packet = new Packet((int)ClientPackets.requestHexGrid))
        {
            packet.Write(Client.instance.myid);
            sendTCPData(packet);
        }
    }

    public static void RequestPlaceBuilding(HexCoordinates coords, Type type)
    {
        if (type == typeof(Headquarter))
        {
            RequestBuildHQ(coords);
            return;
        }
        using (Packet packet = new Packet((int) ClientPackets.requestPlaceBuilding))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            packet.Write(type);
            sendTCPData(packet);
        }
    }

    public static void RequestDestroyBuilding(Building building)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestDestroyBuilding))
        {
            packet.Write(Client.instance.myid);
            packet.Write(building.Cell.coordinates);
            sendTCPData(packet);
        }
    }

    public static void UpgradeBuilding(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestUpgradeBuilding))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void RepairBuilding(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestRepairBuilding))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }
    public static void SalvageBuilding(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestSalvageBuilding))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    private static void RequestBuildHQ(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestBuildHQ))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void UpdatePosition(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int)ClientPackets.positionUpdate))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void RequestJoinTribe(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestJoinTribe))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            packet.Write(Client.instance.Player.Avatar);
            sendTCPData(packet);
        }
    }
    public static void RequestLeaveTribe()
    {
        using (Packet packet = new Packet((int)ClientPackets.requestLeaveTribe))
        {
            packet.Write(Client.instance.myid);
            sendTCPData(packet);
        }
    }

    public static void RequestCreateGuild(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestCreateGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void RequestLeaveGuild(byte guildId, HexCoordinates coords)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestLeaveGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildId);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void RequestJoinGuild(byte guildToJoinId, HexCoordinates coords)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestJoinGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildToJoinId);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void RequestGuildDonation(byte guildId, byte tribeId, SortedDictionary<RessourceType, int> donationMap)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestGuildDonation))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildId);
            packet.Write(tribeId);
            
            // Accounts for both keys and values
            packet.Write(donationMap.Count);    
            
            // resType0, value0, resType1, value1, ..., resTypeN, valueN
            for (int i = 0; i < donationMap.Count; ++i)
            {
                packet.Write((int)donationMap.ElementAt(i).Key);
                packet.Write(donationMap.ElementAt(i).Value);
            }
            
            sendTCPData(packet);
        }
    }

    public static void RequestDepositToGuild(byte guildId, byte tribeId, Dictionary<RessourceType, int> amountMap)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestDepositToGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildId);
            packet.Write(tribeId);
            
            packet.Write(amountMap.Count);
            foreach (KeyValuePair<RessourceType, int> pair in amountMap)
            {
                packet.Write((int)pair.Key);
                packet.Write(pair.Value);
            }
            
            sendTCPData(packet);
        }
    }

    public static void RequestWithdrawalFromGuild(byte guildId, byte tribeId, Dictionary<RessourceType, int> amountMap)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestWithdrawalFromGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildId);
            packet.Write(tribeId);
            
            packet.Write(amountMap.Count);
            foreach (KeyValuePair<RessourceType, int> pair in amountMap)
            {
                packet.Write((int)pair.Key);
                packet.Write(pair.Value);
            }
            
            sendTCPData(packet);
        }
    }

    public static void MoveTroops(HexCoordinates source, HexCoordinates dest, TroopType troopType, int amount)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestMoveTroops))
        {
            packet.Write(Client.instance.myid);
            packet.Write(source);
            packet.Write(dest);
            packet.Write((byte)troopType);
            packet.Write(amount);
            sendTCPData(packet);
        }
    }

    public static void Fight(HexCoordinates attacker, HexCoordinates defender)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestFight))
        {
            packet.Write(Client.instance.myid);
            packet.Write(attacker);
            packet.Write(defender);
            sendTCPData(packet);
        }
    }

    public static void RequestHarvest(HexCoordinates coords)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestHarvest))
        {
            
            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void RequestCollect(HexCoordinates coords) 
    {
        using (Packet packet = new Packet((int)ClientPackets.requestCollect)) 
        {

            packet.Write(Client.instance.myid);
            packet.Write(coords);
            sendTCPData(packet);
        }
    }

    public static void RequestChangeAllowedRessource(HexCoordinates originBuilding, HexCoordinates destinationBuilding, RessourceType ressourceType, bool newValue)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChangeAllowedRessource))
        {
            packet.Write(Client.instance.myid);
            packet.Write(originBuilding);
            packet.Write(destinationBuilding);
            packet.Write((byte)ressourceType);
            packet.Write(newValue);
            sendTCPData(packet);
        }
    }

    public static void RequestChangeTroopRecipeOfBarracks(Barracks barracks, TroopType newTroop)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChangeTroopRecipeOfBarracks))
        {
            packet.Write(Client.instance.myid);
            packet.Write(barracks.Cell.coordinates);
            packet.Write((byte)newTroop);
            sendTCPData(packet);
        }
    }

    public static void RequestChangeOrderOfStrategy(HexCoordinates coords, int oldIndex, int newIndex, bool usePlayerTroopInventory)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChangeOrderOfStrategy))
        {
            packet.Write(Client.instance.myid);
            packet.Write(usePlayerTroopInventory);
            packet.Write(coords);
            packet.Write(oldIndex);
            packet.Write(newIndex);
            sendTCPData(packet);
        }
    }

    public static void RequestChangeActiveOfStrategy(HexCoordinates coordinates, TroopType type, bool newValue)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChangeActiveOfStrategyBuilding))
        {
            packet.Write(Client.instance.myid);
            packet.Write(coordinates);
            packet.Write((byte)type);
            packet.Write(newValue);
            sendTCPData(packet);
        }
    }

    public static void RequestChangeActiveOfStrategy(TroopType type, bool newValue)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChangeActiveOfStrategyPlayer))
        {
            packet.Write(Client.instance.myid);
            packet.Write((byte)type);
            packet.Write(newValue);
            sendTCPData(packet);
        }
    }

    public static void RequestMoveRessources(InventoryBuilding origin, InventoryBuilding destination, RessourceType type, int amount)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestMoveRessource))
        {
            packet.Write(Client.instance.myid);
            packet.Write(origin.Cell.coordinates);
            packet.Write(destination.Cell.coordinates);
            packet.Write((byte)type);
            packet.Write(amount);
            sendTCPData(packet);
        }
    }

    public static void RequestUpdateRessourceLimit(InventoryBuilding building, RessourceType type, int newValue)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChangeRessourceLimit))
        {
            packet.Write(Client.instance.myid);
            packet.Write(building.Cell.coordinates);
            packet.Write((byte)type);
            packet.Write(newValue);
            sendTCPData(packet);
        }
    }

    public static void RequestRecipeChange(MultiRefineryBuilding building, RessourceType type)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestRecipeChange))
        {
            packet.Write(Client.instance.myid);
            packet.Write(building.Cell.coordinates);
            packet.Write((byte) type);
            sendTCPData(packet);
        }
    }

    public static void RequestUpdateMarketRessource(Market market, RessourceType type, bool isInput)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestUpdateMarketRessource))
        {
            packet.Write(Client.instance.myid);
            packet.Write(market.Cell.coordinates);
            packet.Write((byte)type);
            packet.Write(isInput);
            sendTCPData(packet);
        }
    }

    public static void RequestBoostBaseResourceSelectionGuild(GuildHouse guildHouse, RessourceType type)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestBoostBaseResourceSelectionGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildHouse.Cell.coordinates);
            packet.Write((byte)type);
            sendTCPData(packet);
        }
    }
    
    public static void RequestBoostAdvancedResourceSelectionGuild(GuildHouse guildHouse, RessourceType type)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestBoostAdvancedResourceSelectionGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildHouse.Cell.coordinates);
            packet.Write((byte)type);
            sendTCPData(packet);
        }
    }
    
    public static void RequestBoostWeaponSelectionGuild(GuildHouse guildHouse, RessourceType type)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestBoostWeaponSelectionGuild))
        {
            packet.Write(Client.instance.myid);
            packet.Write(guildHouse.Cell.coordinates);
            packet.Write((byte)type);
            sendTCPData(packet);
        }
    }
    
    public static void sendChatMessage(String message, int chatType)
    {

        using (Packet packet = new Packet((int)ClientPackets.sendChatMessage))
        {
            packet.Write(Client.instance.myid);
            packet.Write(message);
            packet.Write(chatType);
            sendTCPData(packet);
        }
    }
    public static void requestChatHistory()
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChatHistory))
        {
            packet.Write(Client.instance.myid);
            sendTCPData(packet);
        }
    }

    public static void sendMovementQuestProgress(int distance)
    {
       using (Packet packet = new Packet((int)ClientPackets.sendMovementQuestProgress))
        {
            packet.Write(Client.instance.myid);
            packet.Write(distance);
            sendTCPData(packet);
        }
    }

    public static void sendBuildingQuestProgress(Type buildingType)
    {
       using (Packet packet = new Packet((int)ClientPackets.sendBuildingQuestProgress))
        {
            packet.Write(Client.instance.myid);
            packet.Write(buildingType);
            sendTCPData(packet);
        }
    }

    public static void sendRessourceQuestProgress(HexCoordinates hexCoordinates)
    {
       using (Packet packet = new Packet((int)ClientPackets.sendRessourceQuestProgress))
        {
            packet.Write(Client.instance.myid);
            packet.Write(hexCoordinates);
            sendTCPData(packet);
        }
    }
    public static void RequestChangePriority(int prio, bool increase)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestPrioChange))
        {
            packet.Write(Client.instance.myid);
            packet.Write(prio);
            packet.Write(increase);
            sendTCPData(packet);
        }
    }

    public static void RequestResearch(int researchCode)
    {
        using (Packet packet = new Packet((int) ClientPackets.requestResearch))
        {
            packet.Write(Client.instance.myid);
            packet.Write(researchCode);
            sendTCPData(packet);
        }
    }

    #endregion

}
