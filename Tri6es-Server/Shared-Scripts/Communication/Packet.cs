using System;
using System.Collections.Generic;
using System.Text;
using Shared.HexGrid;
using Shared.DataTypes;
using Shared.Structures;
using Shared.Game;

namespace Shared.Communication
{
    /// <summary>Sent from server to client.</summary>
    public enum ServerPackets
    {
        welcome,
        ping,
        initGameLogic,
        sendStructure,
        gameTick,
        upgradeBuilding,
        repairBuilding,
        broadcastPlayer,
        broadcastTribe,
        applyBuild,
        applyBuildHQ,
        broadcastMoveTroops,
        broadcastFight,
        broadcastHarvest,
        broadcastChangeAllowedRessource,
        broadcastChangeTroopRecipeOfBarracks,
        broadcastChangeStrategyOfProtectedBuilding,
        broadcastChangeStrategyOfName,
        broadcastChangeStrategyActivePlayer,
        broadcastChangeStrategyActiveBuilding,
        broadcastMoveRessources,
        broadcastChangeRessourceLimit,
        broadcastRecipeChange,
        broadcastUpdateMarketRessource,
        broadcastDestroyBuilding,
        testBuilding,
        sendChatHistory,
        broadcastChatMessage,
        sendQuests,
        salvageBuilding,
        prioChange,
        broadcastWeather,
        createGuild,
        leaveGuild,
        joinGuild,
        guildDonation,
        depositToGuild,
        withdrawalFromGuild,
        boostBaseResourceSelectionGuild,
        boostAdvancedResourceSelectionGuild,
        boostWeaponSelectionGuild,
        broadcastCollect,
        broadcastResearch,
    }

    /// <summary>Sent from client to server.</summary>
    public enum ClientPackets
    {
        welcomeReceived,
        ping,
        requestHexGrid,
        requestPlaceBuilding,
        requestUpgradeBuilding,
        requestRepairBuilding,
        positionUpdate,
        requestBuildHQ,
        requestJoinTribe,
        requestMoveTroops,
        requestFight,
        requestHarvest,
        requestChangeAllowedRessource,
        requestChangeTroopRecipeOfBarracks,
        requestChangeOrderOfStrategy,
        requestChangeActiveOfStrategyPlayer,
        requestChangeActiveOfStrategyBuilding,
        requestChangeRessourceLimit,
        requestMoveRessource,
        requestUpdateMarketRessource,
        requestDestroyBuilding,
        requestRecipeChange,
        requestChatHistory,
        sendChatMessage,
        sendRessourceQuestProgress,
        sendBuildingQuestProgress,
        sendMovementQuestProgress,
        requestLeaveTribe,
        requestSalvageBuilding,
        requestPrioChange,
        requestCreateGuild,
        requestLeaveGuild,
        requestJoinGuild,
        requestGuildDonation,
        requestDepositToGuild,
        requestWithdrawalFromGuild,
        requestBoostBaseResourceSelectionGuild,
        requestBoostAdvancedResourceSelectionGuild,
        requestBoostWeaponSelectionGuild,
        requestCollect,
        requestResearch,
    }

    public class Packet : IDisposable
    {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        /// <summary>Creates a new empty packet (without an ID).</summary>
        public Packet()
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0
        }

        /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
        /// <param name="_id">The packet ID.</param>
        public Packet(int _id)
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0

            Write(_id); // Write packet id to the buffer
        }

        /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
        /// <param name="_data">The bytes to add to the packet.</param>
        public Packet(byte[] _data)
        {
            buffer = new List<byte>(); // Intitialize buffer
            readPos = 0; // Set readPos to 0

            SetBytes(_data);
        }

        #region Functions
        /// <summary>Sets the packet's content and prepares it to be read.</summary>
        /// <param name="_data">The bytes to add to the packet.</param>
        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = buffer.ToArray();
        }

        /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning
        }

        /// <summary>Inserts the given int at the start of the buffer.</summary>
        /// <param name="_value">The int to insert.</param>
        public void InsertInt(int _value)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(_value)); // Insert the int at the start of the buffer
        }

        /// <summary>Gets the packet's content in array form.</summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        /// <summary>Gets the length of the packet's content.</summary>
        public int Length()
        {
            return buffer.Count; // Return the length of buffer
        }

        /// <summary>Gets the length of the unread data contained in the packet.</summary>
        public int UnreadLength()
        {
            return Length() - readPos; // Return the remaining length (unread)
        }

        /// <summary>Resets the packet instance to allow it to be reused.</summary>
        /// <param name="_shouldReset">Whether or not to reset the packet.</param>
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear(); // Clear buffer
                readableBuffer = null;
                readPos = 0; // Reset readPos
            }
            else
            {
                readPos -= 4; // "Unread" the last read int
            }
        }
        #endregion

        #region Write Data

        #region Write Primitives
        /// <summary>Adds a byte to the packet.</summary>
        /// <param name="_value">The byte to add.</param>
        public void Write(byte _value)
        {
            buffer.Add(_value);
        }
        /// <summary>Adds an array of bytes to the packet.</summary>
        /// <param name="_value">The byte array to add.</param>
        public void Write(byte[] _value)
        {
            buffer.AddRange(_value);
        }
        /// <summary>Adds a short to the packet.</summary>
        /// <param name="_value">The short to add.</param>
        public void Write(short _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a ushort to the packet.</summary>
        /// <param name="_value">The ushort to add.</param>
        public void Write(ushort _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds an int to the packet.</summary>
        /// <param name="_value">The int to add.</param>
        public void Write(int _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds an uint to the packet.</summary>
        /// <param name="_value">The uint to add.</param>
        public void Write(uint _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds an uint array to the packet.</summary>
        /// <param name="_value">The uint array to add.</param>
        public void Write(uint[] _value)
        {
            Write(_value.Length);
            for (int i = 0; i < _value.Length; i++)
            {
                buffer.AddRange(BitConverter.GetBytes(_value[i]));
            }
        }
        /// <summary>Adds a long to the packet.</summary>
        /// <param name="_value">The long to add.</param>
        public void Write(long _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a float to the packet.</summary>
        /// <param name="_value">The float to add.</param>
        public void Write(float _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a bool to the packet.</summary>
        /// <param name="_value">The bool to add.</param>
        public void Write(bool _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }
        /// <summary>Adds a string to the packet.</summary>
        /// <param name="_value">The string to add.</param>
        public void Write(string _value)
        {
            Write(_value.Length); // Add the length of the string to the packet
            buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
        }
        #endregion

        #region Write HexStuff

        /// <summary>Adds a HexCoordinates to the packet.</summary>
        /// <param name="_value">The HexCoordinate to add.</param>
        public void Write(HexCoordinates _value)
        {
            Write(_value.X);
            Write(_value.Z);
        }
        /// <summary>Adds a HexCellData to the packet.</summary>
        /// <param name="_value">The HexCellData to add.</param>
        public void Write(HexCellBiome _value)
        {
            Write((byte)_value);
        }
        /// <summary>Adds a HexCellData to the packet.</summary>
        /// <param name="_value">The HexCellData to add.</param>
        public void Write(HexCellData _value)
        {
            Write(_value.Elevation);
            Write(_value.Biome);
            Write(_value.WaterDepth);
        }

        /// <summary>Adds a HexCell to the packet.</summary>
        /// <param name="cell">The HexCell to add.</param>
        public void Write(HexCell cell)
        {
            Write(cell.coordinates);
            Write(cell.Data);
            Write(cell.Protectors.Count);
            foreach (HexCoordinates protector in cell.Protectors)
            {
                Write(protector);
            }
        }

        /// <summary>Adds a HexCell[] to the packet.</summary>
        /// <param name="_value">The HexCell[] to add.</param>
        public void Write(HexCell[] _value)
        {
            Write(_value.Length);
            foreach (HexCell cell in _value)
            {
                Write(cell);
            }
        }
        /// <summary>Adds a HexGrid to the packet.</summary>
        /// <param name="_value">The HexGrid to add.</param>
        public void Write(HexGrid.HexGrid _value)
        {
            Write(_value.chunkCountX);
            Write(_value.chunkCountZ);

            Write(_value.cornerLon);
            Write(_value.cornerLat);
            Write(_value.deltaLon);
            Write(_value.deltaLat);

            Write(_value.cells);
            List<Building> buildings = new List<Building>();
            List<InventoryBuilding> inventoryBuildings = new List<InventoryBuilding>();
            foreach (HexCell cell in _value.cells)
            {
                Write(cell.Structure);
                if (cell.Structure is ICartHandler)
                    buildings.Add((Building)cell.Structure);
                if (cell.Structure is InventoryBuilding)
                    inventoryBuildings.Add((InventoryBuilding)cell.Structure);
            }

            //Write Carts
            Write(buildings.Count);
            foreach (Building building in buildings)
            {
                Write(building.Cell.coordinates);
                Write(((ICartHandler)building).Carts);
            }

            //Write Allowed Ressources
            Write(inventoryBuildings.Count);
            foreach (InventoryBuilding building in inventoryBuildings)
            {
                Write(building.Cell.coordinates);
                Write(building.AllowedRessources);
            }
        }
        /// <summary>Adds a List<Cart> to the packet.</summary>
        /// <param name="_value">The List<Cart> to add.</param>
        public void Write(List<Cart> _value)
        {
            Write(_value.Count);
            foreach (Cart cart in _value)
            {
                Write(cart);
            }
        }
        /// <summary>Adds a Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>> to the packet.</summary>
        /// <param name="_value">The List<Cart> to add.</param>
        public void Write(Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>> _value)
        {
            Write(_value.Count);
            foreach (KeyValuePair<InventoryBuilding, Dictionary<RessourceType, bool>> kvp in _value)
            {
                Write(kvp.Key.Cell.coordinates);
                Write(kvp.Value);
            }
        }
        /// <summary>Adds a Dictionary<RessourceType, bool> to the packet.</summary>
        /// <param name="_value">The List<Cart> to add.</param>
        public void Write(Dictionary<RessourceType, bool> _value)
        {
            Write(_value.Count);
            foreach (KeyValuePair<RessourceType, bool> kvp in _value)
            {
                Write((byte)kvp.Key);
                Write(kvp.Value);
            }
        }
        /// <summary>Adds a Cart to the packet.</summary>
        /// <param name="_value">The Cart to add.</param>
        public void Write(Cart _value)
        {
            Write(_value.Inventory);
            Write(_value.Origin.Cell.coordinates);
            Write(_value.Destination.Cell.coordinates);
        }


        #endregion

        #region Write Structures

        /// <summary>Adds a Structure to the packet.</summary>
        /// <param name="_value">The Structure to add.</param>
        public void Write(Structure _value)
        {
            Write(_value.ToByte());
            if (_value == null)
            {
                return;
            }
            if (_value is Collectable)
            {
                Write((Collectable)_value);
            }
            if (_value is Ressource)
            {
                Write((Ressource)_value);
            }
            if (_value is Building)
            {
                Write((Building)_value);
            }
        }
        /// <summary>Adds a Ressource to the packet.</summary>
        /// <param name="_value">The Ressource to add.</param>
        private void Write(Ressource _value)
        {
            Write(_value.Progress);
        }
        private void Write(Collectable _value)
        {
            if(_value is Loot) 
            {
                Write((Loot)_value);
            }
            if (_value is RessourceBuff) 
            {
                Write((RessourceBuff)_value);
            }
            if(_value is TroopBuff) 
            {
                Write((TroopBuff)_value);
            }
        }
        private void Write(Loot loot) 
        {
            Write(loot.amount);
        }
        private void Write(RessourceBuff buff) 
        {
            Write(buff.ctime);
        }
        private void Write(TroopBuff buff)
        {
            Write(buff.ctime);
        }
        /// <summary>Adds a <see cref="Building"/> to the packet.</summary>
        /// <param name="building">The <see cref="Building"/> to add.</param>
        private void Write(Building building)
        {
            Write(building.Tribe);
            Write(building.Level);

            if (building is InventoryBuilding)
            {
                Write((InventoryBuilding)building);
            }
            else if (building is Ruin)
            {
                Write((Ruin)building);
            }
        }

        /// <summary>Adds an <see cref="InventoryBuilding"/> to the packet.</summary>
        /// <param name="building">The <see cref="InventoryBuilding"/> to add.</param>
        private void Write(InventoryBuilding building)
        {
            Write(building.Inventory);

            if (building is ProgressBuilding)
            {
                Write((ProgressBuilding)building);
            }
            else if (building is StorageBuilding)
            {
                Write((StorageBuilding)building);
            }
            else if (building is ProtectedBuilding)
            {
                Write((ProtectedBuilding)building);
            }
        }

        /// <summary>Adds a Ruin to the packet.</summary>
        /// <param name="_value">The Ruin to add.</param>
        private void Write(Ruin building)
        {
            Write(building.BuildingType);
        }

        /// <summary>Adds a <see cref="ProtectedBuilding"/> to the packet.</summary>
        /// <param name="building">The <see cref="ProtectedBuilding"/> to add.</param>
        private void Write(ProtectedBuilding building)
        {
            Write(building.Health);
            Write(building.TroopInventory);

            if (building is AssemblyPoint)
            {
                Write((AssemblyPoint)building);
            }
        }

        /// <summary>Adds a <see cref="AssemblyPoint"/> to the packet.</summary>
        private void Write(AssemblyPoint building)
        {
            Write(building.Cooldown);
            Write(building.Name);
            Write(building.priority);
        }

        /// <summary>Adds a StorageBuilding to the packet.</summary>
        /// <param name="_value">The StorageBuilding to add.</param>
        private void Write(StorageBuilding _value)
        {

        }

        /// <summary>Adds an ProgressBuilding to the packet.</summary>
        /// <param name="building">The ProgressBuilding to add.</param>
        private void Write(ProgressBuilding building)
        {
            Write(building.Progress);
            if (building is ProductionBuilding)
            {
                Write((ProductionBuilding)building);
            }
            else if (building is RefineryBuilding)
            {
                Write((RefineryBuilding)building);
            }
            else if (building is TroopProductionBuilding)
            {
                Write((TroopProductionBuilding)building);
            }
        }

        /// <summary>Adds an ProductionBuilding to the packet.</summary>
        /// <param name="_value">The ProductionBuilding  to add.</param>
        private void Write(ProductionBuilding _value)
        {

        }

        /// <summary>Adds a <see cref="RefineryBuilding"/> to the packet.</summary>
        /// <param name="_value">The <see cref="RefineryBuilding"/> to add.</param>
        private void Write(RefineryBuilding _value)
        {
            if (_value is Market)
                Write((Market)_value);
            if (_value is MultiRefineryBuilding)
                Write((MultiRefineryBuilding)_value);
        }

        /// <summary>Adds a <see cref="MultiRefineryBuilding"/> to the packet.</summary>
        /// <param name="building">The <see cref="MultiRefineryBuilding"/> to add.</param>
        private void Write(MultiRefineryBuilding building)
        {
            Write((byte)building.CurrentOutput);
        }

        /// <summary>Adds a <see cref="TroopProductionBuilding"/> to the packet.</summary>
        /// <param name="building">The <see cref="TroopProductionBuilding"/> to add.</param>
        private void Write(TroopProductionBuilding building)
        {
            Write(building.TroopInventory);

            if (building is Barracks)
                Write((Barracks)building);
        }

        /// <summary>Adds a <see cref="Barracks"/> to the packet.</summary>
        /// <param name="building">The <see cref="Barracks"/> to add.</param>
        private void Write(Barracks building)
        {
            Write((byte)building.OutputTroop);
        }

        /// <summary>Adds a Market to the packet.</summary>
        /// <param name="_value">The Market to add.</param>
        private void Write(Market _value)
        {
            Write((byte)_value.TradeInput);
            Write((byte)_value.TradeOutput);
        }
        /// <summary>Adds an Inventory to the packet.</summary>
        /// <param name="_value">The Inventory to add.</param>
        private void Write(Inventory _value)
        {
            Write(_value.GetContents());
        }
        /// <summary>Adds an Inventory to the packet.</summary>
        /// <param name="_value">The Inventory to add.</param>
        private void Write(BuildingInventory _value)
        {
            Write(_value.GetContents());
            Write(_value.Limits);
            Write(_value.Outgoing);
            Write(_value.Incoming);
        }
        /// <summary>Adds a TrropInventory to the packet.</summary>
        /// <param name="_value">The TroopInventory to add.</param>
        public void Write(TroopInventory _value)
        {
            Write(_value.Troops);
            Write(_value.TroopLimit);
            Write(_value.Strategy);
        }
        /// <summary>Adds a Dictionary to the packet.</summary>
        /// <param name="_value">The Dictionary to add.</param>
        public void Write(IReadOnlyDictionary<RessourceType, int> _value)
        {
            Write(_value.Count);
            foreach (KeyValuePair<RessourceType, int> pair in _value)
            {
                Write((byte)pair.Key);
                Write(pair.Value);
            }
        }
        /// <summary>Adds a Dictionary to the packet.</summary>
        /// <param name="_value">The Dictionary to add.</param>
        public void Write(Dictionary<TroopType, int> _value)
        {
            Write(_value.Count);
            foreach (KeyValuePair<TroopType, int> pair in _value)
            {
                Write((byte)pair.Key);
                Write(pair.Value);
            }
        }
        /// <summary>Adds a List<RessourceType> to the packet.</summary>
        /// <param name="_value">The List<RessourceType> to add.</param>
        public void Write(List<RessourceType> _value)
        {
            Write(_value.Count);
            foreach (RessourceType ressourceType in _value)
            {
                Write((byte)ressourceType);
            }
        }

        public void Write(List<CollectableType> _value)
        {
            Write(_value.Count);
            foreach (CollectableType collectableType in _value)
            {
                Write((byte)collectableType);
            }
        }
        /// <summary>Adds a List<RessourceType> to the packet.</summary>
        /// <param name="_value">The List<RessourceType> to add.</param>
        public void Write(List<Tuple<TroopType, bool>> _value)
        {
            Write(_value.Count);
            foreach (Tuple<TroopType, bool> tuple in _value)
            {
                Write((byte)tuple.Item1);
                Write(tuple.Item2);
            }
        }
        /// <summary>Adds a List<Structure> to the packet.</summary>
        /// <param name="_value">The List<Structure> to add.</param>
        public void Write(List<Structure> _value)
        {
            Write(_value.Count);
            foreach (Structure structure in _value)
            {
                Write(structure);
            }
        }
        /// <summary>Adds a Type to the packet.</summary>
        /// <param name="_value">The Type to add.</param>
        public void Write(Type _value)
        {
            Write(_value.ToByte());
        }

        #endregion

        #region Write GameLogic

        /// <summary>Adds a Tribe to the packet.</summary>
        /// <param name="_value">The Tribe to add.</param>
        public void Write(Tribe _value)
        {
            Write(_value.Id);
            Write(_value.HQ.Cell.coordinates);
        }
        
        /// <summary>
        /// Writes: <br/>
        /// 1. Guild ID (byte) <br/>
        /// 2. Founding Tribe ID (byte) <br/>
        /// 3. Guild Level (int)
        /// 4. Guild Inventory (see Write(GuildInventory))
        /// 5. Progress map size (int)
        /// 6. Progress keys + values (both int)
        /// 7. Number of Tribes (int) <br/>
        /// 8. Each member's ID (each is a byte) <br/>
        /// </summary>
        public void Write(Guild guild)
        {
            Write(guild.Id);
            Write(guild.FoundingMember.Id);
            Write(guild.Level);
            Write(guild.Inventory);
            Write(guild.GetCurrentProgressMap().Count);
            foreach (KeyValuePair<RessourceType, int> pair in guild.GetCurrentProgressMap())
            {
                Write((int)pair.Key);
                Write(pair.Value);
            }
            Write(guild.Members.Count);
            foreach(Tribe member in guild.Members)
            {
                Write(member.Id);
            }
        }

        /// <summary>
        /// 1. Resources map size (int)
        /// 2. Resources keys + values (both int)
        /// </summary>
        /// <param name="guildInventory"></param>
        public void Write(GuildInventory guildInventory)
        {
            Write(guildInventory.Resources.Count);
            foreach (KeyValuePair<RessourceType, int> pair in guildInventory.Resources)
            {
                Write((int)pair.Key);
                Write(pair.Value);
            }
        }

        /// <summary>Adds questlines to the packet.</summary>
        public void Write(List<List<Quest>> questlines)
        {
            Write(questlines.Count);
            foreach (List<Quest> line in questlines)
            {
                Write(line.Count);
                foreach (Quest quest in line)
                {
                    Write(quest);
                }
            }
        }

        /// <summary>Adds a Quest to the packet.</summary>
        public void Write(Quest quest)
        {
            // common info
            Write(quest.Goal);
            Write(quest.Reward);
            Write((byte)quest.RewardType);

            // progress
            Write(quest.isActive);
            Write(quest.Progress);

            // kind
            Write((byte)quest.kind);
            if (quest.IsRessource)
            {
                Write((byte)quest.RessourceProgressType);
            }
            else if (quest.isBuilding)
            {
                Write(quest.BuildingProgressType);
            }
        }
        /// <summary>Adds the ressourcetype to the packet</summary>

        public void Write(RessourceType ressource)
        {

        }

        /// <summary>Adds the feedback to the packet.</summary>
        ///  <param name="ressource"> Which Resourcetype should be in feedback included.</param>
        public void Write(Feedback feedback)
        {
            Write((byte)feedback.feedbackStyle);
            Write(feedback.successfull);

            switch (feedback.feedbackStyle)
            {
                case Feedback.FeedbackStyle.harvest:
                    Write((byte)feedback.resource);
                    Write(feedback.quantity);
                    break;
                case Feedback.FeedbackStyle.build:
                    Write(feedback.message);
                    break;
                case Feedback.FeedbackStyle.upgrade:
                    Write(feedback.message);
                    Write(feedback.type);
                    break;
                case Feedback.FeedbackStyle.destroy:
                    Write(feedback.quantity);
                    Write(feedback.type);
                    break;
                case Feedback.FeedbackStyle.battle:
                    Write(feedback.message);
                    if (feedback.successfull)
                    {
                        Write(feedback.battleLog.attackerTroops);
                        Write(feedback.battleLog.defenderTroops);
                        Write(feedback.battleLog.winnersRest);
                        Write((byte)feedback.battleLog.result);
                        Write(feedback.type);
                        Write(feedback.TribeId);
                        Write(feedback.coordinates);
                    }                
                    
                    break;
                case Feedback.FeedbackStyle.collect: 
                    // TODO: missing collect write
                case Feedback.FeedbackStyle.plainMessage:
                    Write(feedback.message);
                    break;
                default:
                    break;
            }

        }

        #endregion

        #endregion


        #region Read Data

        #region Read Primitives
        /// <summary>Reads a byte from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte ReadByte(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte _value = readableBuffer[readPos]; // Get the byte at readPos' position
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 1; // Increase readPos by 1
                }
                return _value; // Return the byte
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>Reads an array of bytes from the packet.</summary>
        /// <param name="_length">The length of the byte array.</param>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                byte[] _value = buffer.GetRange(readPos, _length).ToArray(); // Get the bytes at readPos' position with a range of _length
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += _length; // Increase readPos by _length
                }
                return _value; // Return the bytes
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>Reads a short from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                short _value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
                if (_moveReadPos)
                {
                    // If _moveReadPos is true and there are unread bytes
                    readPos += 2; // Increase readPos by 2
                }
                return _value; // Return the short
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>Reads a ushort from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public ushort ReadUShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                ushort _value = BitConverter.ToUInt16(readableBuffer, readPos); // Convert the bytes to a ushort
                if (_moveReadPos)
                {
                    // If _moveReadPos is true and there are unread bytes
                    readPos += 2; // Increase readPos by 2
                }
                return _value; // Return the ushort
            }
            else
            {
                throw new Exception("Could not read value of type 'ushort'!");
            }
        }

        /// <summary>Reads an int from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                int _value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>Reads an uint from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public uint ReadUInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                uint _value = BitConverter.ToUInt32(readableBuffer, readPos); // Convert the bytes to an uint
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the int
            }
            else
            {
                throw new Exception("Could not read value of type 'Uint'!");
            }
        }

        /// <summary>Reads an uint array from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public uint[] ReadUIntArray(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                int length = ReadInt(_moveReadPos);
                uint[] array = new uint[length];
                for (int i = 0; i < length; i++)
                {
                    array[i] = ReadUInt(_moveReadPos);
                }
                return array; // Return the uint array
            }
            else
            {
                throw new Exception("Could not read value of type 'Uint Array'!");
            }
        }

        /// <summary>Reads a long from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                long _value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 8; // Increase readPos by 8
                }
                return _value; // Return the long
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>Reads a float from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                float _value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 4; // Increase readPos by 4
                }
                return _value; // Return the float
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>Reads a bool from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                // If there are unread bytes
                bool _value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
                if (_moveReadPos)
                {
                    // If _moveReadPos is true
                    readPos += 1; // Increase readPos by 1
                }
                return _value; // Return the bool
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>Reads a string from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public string ReadString(bool _moveReadPos = true)
        {
            int _length = ReadInt(_moveReadPos); // Get the length of the string
            string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length); // Convert the bytes to a string
            if (_moveReadPos && _value.Length > 0)
            {
                // If _moveReadPos is true string is not empty
                readPos += _length; // Increase readPos by the length of the string
            }
            return _value; // Return the string
        }

        #endregion

        #region  Read HexStuff
        /// <summary>Reads a HexCoordinates from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public HexCoordinates ReadHexCoordinates(bool _moveReadPos = true)
        {
            int x = ReadInt(_moveReadPos);
            int z = ReadInt(_moveReadPos);
            HexCoordinates _value = new HexCoordinates(x, z);
            return _value; // Return the HexCoordinates
        }
        /// <summary>Reads a HexCellBiome from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public HexCellBiome ReadHexCellBiome(bool _moveReadPos = true)
        {
            HexCellBiome _value = (HexCellBiome)ReadByte(_moveReadPos);
            return _value;
        }
        /// <summary>Reads a HexCellData from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public HexCellData ReadHexCellData(bool _moveReadPos = true)
        {
            int Elevation = ReadInt(_moveReadPos);
            HexCellBiome Biome = ReadHexCellBiome(_moveReadPos);
            byte WaterDepth = ReadByte(_moveReadPos);
            HexCellData _value = new HexCellData(Elevation, Biome, WaterDepth);
            return _value;
        }

        /// <summary>Reads a HexCell from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public HexCell ReadHexCell(bool _moveReadPos = true)
        {
            HexCoordinates coordinates = ReadHexCoordinates(_moveReadPos);
            HexCellData data = ReadHexCellData(_moveReadPos);
            int length = ReadInt(_moveReadPos);

            List<HexCoordinates> protectors = new List<HexCoordinates>();
            for (int i = 0; i < length; i++)
            {
                protectors.Add(ReadHexCoordinates(_moveReadPos));
            }

            return new HexCell(coordinates, data, protectors);
        }

        /// <summary>Reads a HexCell[] from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public HexCell[] ReadHexCells(bool _moveReadPos = true)
        {
            int length = ReadInt(_moveReadPos);
            HexCell[] _value = new HexCell[length];
            for (int i = 0; i < length; i++)
            {
                _value[i] = ReadHexCell(_moveReadPos);
            }
            return _value;
        }
        /// <summary>Reads a HexGrid from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public HexGrid.HexGrid ReadHexGrid(bool _moveReadPos = true)
        {
            int chunkCountX = ReadInt(_moveReadPos);
            int chunkCountZ = ReadInt(_moveReadPos);

            float cornerLon = ReadFloat(_moveReadPos);
            float cornerLat = ReadFloat(_moveReadPos);
            float deltaLon = ReadFloat(_moveReadPos);
            float deltaLat = ReadFloat(_moveReadPos);

            HexGrid.HexGrid _value = new HexGrid.HexGrid(chunkCountX, chunkCountZ, cornerLon, cornerLat, deltaLon, deltaLat);

            HexCell[] cells = ReadHexCells(_moveReadPos);

            foreach (HexCell cell in cells)
            {
                Structure structure = ReadStructure();
                cell.Structure = structure;
                if (structure != null)
                    structure.Cell = cell;
            }

            _value.cells = cells;

            for (int i = 0; i < _value.cells.Length; i++)
            {
                int x = i % (_value.cellCountX);
                int z = i / (_value.cellCountX);
                _value.UpdateNeighbors(x, z, i);
                _value.AddCellToChunk(x, z, _value.cells[i]);
            }

            //Read Carts
            int size = ReadInt(_moveReadPos);
            for (int i = 0; i < size; i++)
            {
                HexCoordinates coords = ReadHexCoordinates(_moveReadPos);
                List<Cart> carts = ReadListOfCart(_value, _moveReadPos);
                ((ICartHandler)_value.GetCell(coords).Structure).Carts = carts;
            }

            //Read Allowed Ressources
            int inventoryBuildingCount = ReadInt(_moveReadPos);
            for (int i = 0; i < inventoryBuildingCount; i++)
            {
                HexCoordinates coords = ReadHexCoordinates(_moveReadPos);
                Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>> allowedRessources = ReadAllowedRessources(_value, _moveReadPos);
                ((InventoryBuilding)_value.GetCell(coords).Structure).AllowedRessources = allowedRessources;
            }

            return _value;
        }
        /// <summary>Reads a Cart from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Cart ReadCart(HexGrid.HexGrid grid, bool _moveReadPos = true)
        {
            Inventory Inventory = ReadInventory(_moveReadPos);
            HexCoordinates origin = ReadHexCoordinates(_moveReadPos);
            HexCoordinates destination = ReadHexCoordinates(_moveReadPos);

            return new Cart(
                Inventory,
                (InventoryBuilding)grid.GetCell(origin).Structure,
                (InventoryBuilding)grid.GetCell(destination).Structure
            );
        }
        /// <summary>Reads a List<Cart> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public List<Cart> ReadListOfCart(HexGrid.HexGrid grid, bool _moveReadPos = true)
        {
            int size = ReadInt(_moveReadPos);
            List<Cart> _value = new List<Cart>();
            for (int i = 0; i < size; i++)
            {
                _value.Add(ReadCart(grid, _moveReadPos));
            }
            return _value;
        }
        /// <summary>Reads a Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>> ReadAllowedRessources(HexGrid.HexGrid grid, bool _moveReadPos = true)
        {
            Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>> _value = new Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>>();

            int size = ReadInt(_moveReadPos);
            for (int i = 0; i < size; i++)
            {
                HexCoordinates coords = ReadHexCoordinates(_moveReadPos);
                InventoryBuilding key = (InventoryBuilding)grid.GetCell(coords).Structure;
                Dictionary<RessourceType, bool> dictionary = ReadDictionaryOfRessourceTypeAndBool(_moveReadPos);
                _value.Add(key, dictionary);
            }
            return _value;
        }
        /// <summary>Reads a Dictionary<RessourceType, bool> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Dictionary<RessourceType, bool> ReadDictionaryOfRessourceTypeAndBool(bool _moveReadPos = true)
        {
            Dictionary<RessourceType, bool> _value = new Dictionary<RessourceType, bool>();

            int size = ReadInt(_moveReadPos);
            for (int i = 0; i < size; i++)
            {
                RessourceType ressourceType = (RessourceType)ReadByte(_moveReadPos);
                bool allowed = ReadBool();
                _value.Add(ressourceType, allowed);
            }
            return _value;
        }

        #endregion

        #region Read Structures
        /// <summary>Reads a Structure from the packet directly onto the HexGrid.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Structure ReadStructure(HexGrid.HexGrid grid, bool _moveReadPos = true)
        {
            Structure _value = ReadStructure(_moveReadPos);
            if (_value != null)
            {
                HexCoordinates coordinates = ReadHexCoordinates(_moveReadPos);

                HexCell cell = grid.GetCell(coordinates);
                _value.Cell = cell;
                cell.Structure = _value;
                if (_value is ICartHandler)
                    ((ICartHandler)_value).Carts = ReadListOfCart(grid, _moveReadPos);
            }
            return _value;
        }
        /// <summary>Reads a Structure from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Structure ReadStructure(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();
            if (type == null)
            {
                ReadByte(_moveReadPos);
                return null;
            }
            if (typeof(Building).IsAssignableFrom(type))
                return ReadBuilding(_moveReadPos);
            if (typeof(Ressource).IsAssignableFrom(type))
                return ReadRessource(_moveReadPos);
            if (typeof(Collectable).IsAssignableFrom(type))
                return ReadCollectable(_moveReadPos);

            ReadByte(_moveReadPos);

            Structure _value = (Structure)Activator.CreateInstance(type);

            return _value;
        }
        /// <summary>Reads a Ressource from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Ressource ReadRessource(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            int progress = ReadInt(_moveReadPos);

            Ressource _value = (Ressource)Activator.CreateInstance(type, new object[]{
                null,
                progress
            });
            return _value;
        }
        public Collectable ReadCollectable(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            int time = ReadInt(_moveReadPos);

            Collectable _value = (Collectable)Activator.CreateInstance(type, new object[] {
            null,
            time
            });
            return _value;
        }

        /// <summary>Reads a Building from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Building ReadBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();
            if (typeof(Ruin).IsAssignableFrom(type))
            {
                return ReadRuin(_moveReadPos);
            }
            if (typeof(InventoryBuilding).IsAssignableFrom(type))
            {
                return ReadInventoryBuilding(_moveReadPos);
            }
            else
            {
                ReadByte(_moveReadPos);

                byte Tribe = ReadByte(_moveReadPos);
                byte Level = ReadByte(_moveReadPos);

                return (Building)Activator.CreateInstance(type, new object[]{
                    null,
                    Tribe,
                    Level
                });
            }
        }

        /// <summary>Reads a Ruin from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Ruin ReadRuin(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();

            ReadByte(_moveReadPos);

            byte Tribe = ReadByte(_moveReadPos);
            byte Level = ReadByte(_moveReadPos);
            Type BuildingType = ReadType(_moveReadPos);

            return (Ruin)Activator.CreateInstance(type, new object[]{
                null,
                Tribe,
                Level,
                BuildingType
            });
        }

        /// <summary>Reads an InventoryBuilding from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public InventoryBuilding ReadInventoryBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();
            if (typeof(ProgressBuilding).IsAssignableFrom(type))
                return ReadProgressBuilding(_moveReadPos);
            if (typeof(StorageBuilding).IsAssignableFrom(type))
                return ReadStorageBuilding(_moveReadPos);
            if (typeof(ProtectedBuilding).IsAssignableFrom(type))
                return ReadProtectedBuilding(_moveReadPos);

            ReadByte(_moveReadPos);

            byte Tribe = ReadByte(_moveReadPos);
            byte Level = ReadByte(_moveReadPos);
            BuildingInventory Inventory = ReadBuildingInventory(_moveReadPos);

            return (InventoryBuilding)Activator.CreateInstance(type, new object[]{
                null,
                Tribe,
                Level,
                Inventory
            });
        }

        /// <summary>Reads a ProtectedBuilding from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public ProtectedBuilding ReadProtectedBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();
            if (typeof(AssemblyPoint).IsAssignableFrom(type))
                return ReadAssemblyPoint(_moveReadPos);

            ReadByte(_moveReadPos);

            byte tribe = ReadByte(_moveReadPos);
            byte level = ReadByte(_moveReadPos);
            BuildingInventory inventory = ReadBuildingInventory(_moveReadPos);
            byte health = ReadByte(_moveReadPos);
            TroopInventory troops = ReadTroopInventory(_moveReadPos);

            return (ProtectedBuilding)Activator.CreateInstance(type, new object[]{
                null,
                tribe,
                level,
                inventory,
                health,
                troops
            });
        }

        /// <summary>Reads an Assembly Point from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public AssemblyPoint ReadAssemblyPoint(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            byte tribe = ReadByte(_moveReadPos);
            byte level = ReadByte(_moveReadPos);
            BuildingInventory inventory = ReadBuildingInventory(_moveReadPos);
            byte health = ReadByte(_moveReadPos);
            TroopInventory troops = ReadTroopInventory(_moveReadPos);
            int cooldown = ReadInt();
            string name = ReadString();
            int priority = ReadInt();

            return (AssemblyPoint)Activator.CreateInstance(type, new object[]{
                null,
                tribe,
                level,
                inventory,
                health,
                troops,
                cooldown,
                name,
                priority
            });
        }

        /// <summary>Reads an StorageBuilding from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public InventoryBuilding ReadStorageBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            byte Tribe = ReadByte(_moveReadPos);
            byte Level = ReadByte(_moveReadPos);
            BuildingInventory Inventory = ReadBuildingInventory(_moveReadPos);

            return (InventoryBuilding)Activator.CreateInstance(type, new object[]{
                null,
                Tribe,
                Level,
                Inventory
            });
        }
        /// <summary>Reads an ProgressBuilding from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public ProgressBuilding ReadProgressBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();
            if (typeof(ProductionBuilding).IsAssignableFrom(type))
                return ReadProductionBuilding(_moveReadPos);
            if (typeof(RefineryBuilding).IsAssignableFrom(type))
                return ReadRefineryBuilding(_moveReadPos);
            if (typeof(TroopProductionBuilding).IsAssignableFrom(type))
                return ReadTroopProductionBuilding(_moveReadPos);

            ReadByte(_moveReadPos);

            byte Tribe = ReadByte(_moveReadPos);
            byte Level = ReadByte(_moveReadPos);
            BuildingInventory Inventory = ReadBuildingInventory(_moveReadPos);
            int Progress = ReadInt(_moveReadPos);

            return (ProgressBuilding)Activator.CreateInstance(type, new object[]{
                null,
                Tribe,
                Level,
                Inventory,
                Progress
            });
        }

        /// <summary>Reads an ProductionBuilding from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public ProductionBuilding ReadProductionBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            byte Tribe = ReadByte(_moveReadPos);
            byte Level = ReadByte(_moveReadPos);
            BuildingInventory Inventory = ReadBuildingInventory(_moveReadPos);
            int Progress = ReadInt(_moveReadPos);

            return (ProductionBuilding)Activator.CreateInstance(type, new object[]{
                null,
                Tribe,
                Level,
                Inventory,
                Progress
            });
        }

        /// <summary>Reads an RefineryBuilding from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public RefineryBuilding ReadRefineryBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();

            if (typeof(MultiRefineryBuilding).IsAssignableFrom(type))
                return ReadMultiRefineryBuilding(_moveReadPos);
            if (typeof(Market).IsAssignableFrom(type))
                return ReadMarket(_moveReadPos);

            ReadByte(_moveReadPos);
            byte Tribe = ReadByte(_moveReadPos);
            byte Level = ReadByte(_moveReadPos);
            BuildingInventory Inventory = ReadBuildingInventory(_moveReadPos);
            int Progress = ReadInt(_moveReadPos);

            return (RefineryBuilding)Activator.CreateInstance(type, new object[]{
                null,
                Tribe,
                Level,
                Inventory,
                Progress
            });
        }

        /// <summary>Reads a <see cref="MultiRefineryBuilding"/> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public MultiRefineryBuilding ReadMultiRefineryBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            byte tribe = ReadByte(_moveReadPos);
            byte level = ReadByte(_moveReadPos);
            BuildingInventory inventory = ReadBuildingInventory(_moveReadPos);
            int progress = ReadInt(_moveReadPos);
            RessourceType output = (RessourceType)ReadByte(_moveReadPos);

            return (MultiRefineryBuilding)Activator.CreateInstance(type, new object[] {
                null,
                tribe,
                level,
                inventory,
                progress,
                output
            });
        }

        /// <summary>Reads a <see cref="TroopProductionBuilding"/> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public TroopProductionBuilding ReadTroopProductionBuilding(bool _moveReadPos = true)
        {
            Type type = ReadByte(false).ToType();
            if (typeof(Barracks).IsAssignableFrom(type))
                return ReadBarracks(_moveReadPos);

            ReadByte(_moveReadPos);

            byte tribe = ReadByte(_moveReadPos);
            byte level = ReadByte(_moveReadPos);
            BuildingInventory inventory = ReadBuildingInventory(_moveReadPos);
            int progress = ReadInt(_moveReadPos);
            TroopInventory troops = ReadTroopInventory(_moveReadPos);

            return (TroopProductionBuilding)Activator.CreateInstance(type, new object[] {
                null,
                tribe,
                level,
                inventory,
                progress,
                troops
            });
        }

        /// <summary>Reads a <see cref="Barracks"/> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Barracks ReadBarracks(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            byte tribe = ReadByte(_moveReadPos);
            byte level = ReadByte(_moveReadPos);
            BuildingInventory inventory = ReadBuildingInventory(_moveReadPos);
            int progress = ReadInt(_moveReadPos);
            TroopInventory troops = ReadTroopInventory(_moveReadPos);
            TroopType output = (TroopType)ReadByte(_moveReadPos);

            return (Barracks)Activator.CreateInstance(type, new object[]{
                null,
                tribe,
                level,
                inventory,
                progress,
                troops,
                output
            });
        }

        /// <summary>Reads a Market from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Market ReadMarket(bool _moveReadPos = true)
        {
            Type type = ReadByte(_moveReadPos).ToType();

            byte Tribe = ReadByte(_moveReadPos);
            byte Level = ReadByte(_moveReadPos);
            BuildingInventory Inventory = ReadBuildingInventory(_moveReadPos);
            int Progress = ReadInt(_moveReadPos);
            RessourceType TradeInput = (RessourceType)ReadByte(_moveReadPos);
            RessourceType TradeOutput = (RessourceType)ReadByte(_moveReadPos);

            return (Market)Activator.CreateInstance(type, new object[]{
                null,
                Tribe,
                Level,
                Inventory,
                Progress,
                TradeInput,
                TradeOutput
            });
        }

        /// <summary>Reads an Inventory from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Inventory ReadInventory(bool _moveReadPos = true)
        {
            Dictionary<RessourceType, int> storage = ReadDictionaryRessourceTypeInt(_moveReadPos);
            return new Inventory(storage);
        }

        /// <summary>Reads a BuildingInventory from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public BuildingInventory ReadBuildingInventory(bool _moveReadPos = true)
        {
            Dictionary<RessourceType, int> storage = ReadDictionaryRessourceTypeInt(_moveReadPos);
            Dictionary<RessourceType, int> limits = ReadDictionaryRessourceTypeInt(_moveReadPos);
            List<RessourceType> outgoing = ReadRessourceTypes(_moveReadPos);
            List<RessourceType> incoming = ReadRessourceTypes(_moveReadPos);

            return new BuildingInventory(limits, storage, outgoing, incoming);
        }
        /// <summary>Reads an TroopInventory from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public TroopInventory ReadTroopInventory(bool _moveReadPos = true)
        {
            Dictionary<TroopType, int> troops = ReadDictionaryTroopTypeInt(_moveReadPos);
            int troopLimit = ReadInt(_moveReadPos);
            List<Tuple<TroopType, bool>> strategy = ReadListTroopTypeBool(_moveReadPos);

            TroopInventory _value = new TroopInventory(troops, troopLimit, strategy);

            return _value;
        }
        /// <summary>Reads a Dictionary<RessourceType, int> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Dictionary<RessourceType, int> ReadDictionaryRessourceTypeInt(bool _moveReadPos = true)
        {
            Dictionary<RessourceType, int> _value = new Dictionary<RessourceType, int>();
            int count = ReadInt(_moveReadPos);
            while (count > 0)
            {
                RessourceType ressourceType = (RessourceType)ReadByte(_moveReadPos);
                int amount = ReadInt(_moveReadPos);
                _value.Add(ressourceType, amount);
                count--;
            }
            return _value;
        }
        /// <summary>Reads a Dictionary<TroopType, int> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public Dictionary<TroopType, int> ReadDictionaryTroopTypeInt(bool _moveReadPos = true)
        {
            Dictionary<TroopType, int> _value = new Dictionary<TroopType, int>();
            int count = ReadInt(_moveReadPos);
            while (count > 0)
            {
                TroopType troopType = (TroopType)ReadByte(_moveReadPos);
                int amount = ReadInt(_moveReadPos);
                _value.Add(troopType, amount);
                count--;
            }
            return _value;
        }
        /// <summary>Reads a List<RessourceType> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public List<RessourceType> ReadRessourceTypes(bool _moveReadPos = true)
        {
            List<RessourceType> _value = new List<RessourceType>();
            int count = ReadInt(_moveReadPos);
            while (count > 0)
            {
                RessourceType ressourceType = (RessourceType)ReadByte(_moveReadPos);
                _value.Add(ressourceType);
                count--;
            }
            return _value;
        }

        public List<CollectableType> ReadCollectableTypes(bool _moveReadPos = true)
        {
            List<CollectableType> _value = new List<CollectableType>();
            int count = ReadInt(_moveReadPos);
            while (count > 0)
            {
                CollectableType collectableType = (CollectableType)ReadByte(_moveReadPos);
                _value.Add(collectableType);
                count--;
            }
            return _value;
        }
        /// <summary>Reads a List<Tuple<TroopType, bool>> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public List<Tuple<TroopType, bool>> ReadListTroopTypeBool(bool _moveReadPos = true)
        {
            List<Tuple<TroopType, bool>> _value = new List<Tuple<TroopType, bool>>();
            int count = ReadInt(_moveReadPos);
            while (count > 0)
            {
                TroopType troopType = (TroopType)ReadByte(_moveReadPos);
                bool cycling = ReadBool(_moveReadPos);
                _value.Add(new Tuple<TroopType, bool>(troopType, cycling));
                count--;
            }
            return _value;
        }
        /// <summary>Reads a List<Structure> from the packet.</summary>
        /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
        public List<Structure> ReadStructures(bool _moveReadPos = true)
        {
            List<Structure> _value = new List<Structure>();
            int count = ReadInt(_moveReadPos);
            while (count > 0)
            {
                Structure structure = ReadStructure(_moveReadPos);
                _value.Add(structure);
                count--;
            }
            return _value;
        }
        public Type ReadType(bool _moveReadPos = true)
        {
            return ReadByte(_moveReadPos).ToType();
        }

        public GuildInventory ReadGuildInventory()
        {
            GuildInventory guildInventory = new GuildInventory();

            // Resources
            int mapSize = ReadInt();
            for (int i = 0; i < mapSize; ++i)
            {
                RessourceType type = (RessourceType) ReadInt();
                int amount = ReadInt();

                if (guildInventory.Resources.ContainsKey(type))
                {
                    guildInventory.Resources[type] = amount;
                }
            }

            return guildInventory;
        }

        #endregion

        /// <summary>Reads Questlines from the packet.</summary>
        public List<List<Quest>> ReadQuestlines(bool movePos = true)
        {
            List<List<Quest>> questlines = new List<List<Quest>>();

            int outerCount = ReadInt(movePos);
            for (int i = 0; i < outerCount; i++)
            {
                List<Quest> line = new List<Quest>();

                int innerCount = ReadInt();
                for (int j = 0; j < innerCount; j++)
                {
                    line.Add(ReadQuest(movePos));
                }

                questlines.Add(line);
            }

            return questlines;
        }

        /// <summary>Reads a Quest from the packet.</summary>
        public Quest ReadQuest(bool movePos = true)
        {
            // common info
            int goal = ReadInt(movePos);
            int reward = ReadInt(movePos);
            RessourceType rewardType = (RessourceType)ReadByte(movePos);

            // progress
            bool isActive = ReadBool(movePos);
            int progress = ReadInt(movePos);

            // kind
            switch ((Quest.Kind)ReadByte(movePos))
            {
                case Quest.Kind.Ressource:
                    RessourceType ressourceProgressType = (RessourceType)ReadByte();
                    return Quest.CreateRessource(goal, ressourceProgressType, reward, rewardType, isActive, progress);

                case Quest.Kind.Building:
                    Type buildingProgressType = ReadType();
                    return Quest.CreateBuilding(goal, buildingProgressType, reward, rewardType, isActive, progress);

                case Quest.Kind.Movement:
                default:
                    // we default to movement quest
                    return Quest.CreateMovement(goal, reward, rewardType, isActive, progress);
            }
        }

        #endregion

        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Handles the mapping and conversion between Structures (eg. HQ, Tree, ...) and their corresponding byte values
    /// that are sent over the network. 
    /// </summary>
    internal static class StructureTypeExtension
    {
        private static List<Tuple<Type, byte>> mapping = new List<Tuple<Type, byte>>
        {
            new Tuple<Type, byte>(typeof(Woodcutter), 0),
            new Tuple<Type, byte>(typeof(Tree), 1),
            new Tuple<Type, byte>(typeof(Rock), 2),
            new Tuple<Type, byte>(typeof(Fish), 3),
            new Tuple<Type, byte>(typeof(Scrub), 4),
            new Tuple<Type, byte>(typeof(Grass), 5),
            new Tuple<Type, byte>(typeof(Quarry), 6),
            new Tuple<Type, byte>(typeof(LandRoad), 7),
            new Tuple<Type, byte>(typeof(CoalOre), 9),
            new Tuple<Type, byte>(typeof(Wheat), 10),
            new Tuple<Type, byte>(typeof(Storage), 11 ),
            new Tuple<Type, byte>(typeof(Headquarter), 12),
            new Tuple<Type, byte>(typeof(CoalMine), 13),
            new Tuple<Type, byte>(typeof(Smelter), 14),
            new Tuple<Type, byte>(typeof(Fisher), 16),
            new Tuple<Type, byte>(typeof(Bridge), 17),
            new Tuple<Type, byte>(typeof(Market), 18),
            new Tuple<Type, byte>(typeof(WheatFarm), 19),
            new Tuple<Type, byte>(typeof(CowFarm), 20),
            new Tuple<Type, byte>(typeof(Bakery), 21),
            new Tuple<Type, byte>(typeof(Tanner), 22),
            new Tuple<Type, byte>(typeof(Butcher), 23),
            new Tuple<Type, byte>(typeof(Cow), 24),
            new Tuple<Type, byte>(typeof(Barracks), 25),
            new Tuple<Type, byte>(typeof(Tower), 26),
            new Tuple<Type, byte>(typeof(WeaponSmith), 27),
            new Tuple<Type, byte>(typeof(ArmorSmith), 28),
            new Tuple<Type, byte>(typeof(Workshop), 29),
            new Tuple<Type, byte>(typeof(IronMine), 30),
            new Tuple<Type, byte>(typeof(AssemblyPoint), 31),
            new Tuple<Type, byte>(typeof(Ruin), 32),
            new Tuple<Type, byte>(typeof(Library), 33),
            new Tuple<Type, byte>(typeof(GuildHouse), 34),
            new Tuple<Type, byte>(typeof(Loot), 35),
            new Tuple<Type, byte>(typeof(CoalBuff), 36),
            new Tuple<Type, byte>(typeof(CowBuff), 37),
            new Tuple<Type, byte>(typeof(StoneBuff), 38),
            new Tuple<Type, byte>(typeof(WheatBuff), 39),
            new Tuple<Type, byte>(typeof(WoodBuff), 40),
            new Tuple<Type, byte>(typeof(ArcherBuff), 41),
            new Tuple<Type, byte>(typeof(KnightBuff), 42),
            new Tuple<Type, byte>(typeof(ScoutBuff), 43),
            new Tuple<Type, byte>(typeof(SEBuff), 44),
            new Tuple<Type, byte>(typeof(SpearmanBuff), 45),
        };

        internal static byte ToByte(this Structure structure)
        {
            if (structure == null)
                return 255;
            return mapping.Find(elem => elem.Item1 == structure.GetType()).Item2;
        }

        internal static byte ToByte(this Type type)
        {
            if (type == null)
                return 255;
            return mapping.Find(elem => elem.Item1 == type).Item2;
        }

        internal static Type ToType(this byte b)
        {
            if (b == 255)
            {
                return null;
            }
            return mapping.Find(elem => elem.Item2 == b).Item1;
        }
    }
}
