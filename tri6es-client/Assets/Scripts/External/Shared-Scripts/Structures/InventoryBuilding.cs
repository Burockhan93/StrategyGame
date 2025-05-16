using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    public abstract class InventoryBuilding : Building, ICartHandler
    {
        /// <summary>Current ressource limit of the building.</summary>
        public Dictionary<RessourceType, int> CurrentLimit => InventoryLimits.ToDictionary(el => el.Key, el => el.Value[Level - 1]);

        /// <summary>Defines ressource inventory limits per level for the building.</summary>
        public abstract Dictionary<RessourceType, int[]> InventoryLimits { get; }

        public BuildingInventory Inventory;

        public Dictionary<InventoryBuilding, Tuple<HexDirection, int, int>> ConnectedInventories;

        public Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>> AllowedRessources;

        public List<Cart> Carts { get; set; }

        public virtual byte MaxCartCount { get; } = 1;

        public InventoryBuilding() : base()
        {
            this.Inventory = new BuildingInventory(CurrentLimit);
            this.ConnectedInventories = new Dictionary<InventoryBuilding, Tuple<HexDirection, int, int>>();
            this.AllowedRessources = new Dictionary<InventoryBuilding, Dictionary<RessourceType, bool>>();
            this.Carts = new List<Cart>();
            for (int i = 0; i < this.MaxCartCount; i++)
            {
                this.Carts.Add(new Cart(this));
            }
        }

        public InventoryBuilding(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory
        ) : base(cell, tribe, level)
        {
            this.Inventory = inventory;
            this.ConnectedInventories = new Dictionary<InventoryBuilding, Tuple<HexDirection, int, int>>();
            this.Carts = new List<Cart>();
        }

        public override void DoTick()
        {
            HandleCarts();
        }

        public override bool IsPlaceableAt(HexCell cell)
        {
            // inventory buildings cant be placed in water
            if (cell.Data.Biome == HexCellBiome.WATER)
                return false;
            else
                return base.IsPlaceableAt(cell);
        }

        public override void Upgrade()
        {
            base.Upgrade();
            Inventory.UpdateRessourceLimits(CurrentLimit);
        }

        public override void Downgrade()
        {
            base.Upgrade();
            Inventory.UpdateRessourceLimits(CurrentLimit);
        }

        private bool TrySendCart(Cart cart)
        {
            if (cart.HasMoved)
            {
                return false;
            }
            KeyValuePair<InventoryBuilding, Tuple<HexDirection, int, int>> destination;
            try
            {
                destination = this.FindDestination();
            }
            catch(Exception e)
            {
                return false;
            }

            if(destination.Key.FillCart(cart, this))
            {
                HexCell neighbor = this.Cell.GetNeighbor(destination.Value.Item1);
                if (neighbor != null && neighbor.Structure is Road)
                {
                    this.Carts.Remove(cart);
                    cart.Destination = destination.Key;
                    ((Road)neighbor.Structure).AddCart(cart);
                    return true;
                }
            }
            return false;
        }



        public bool UnloadCart(Cart cart)
        {
            // transfer cart contents
            cart.Inventory.MoveAllInto(this.Inventory);

            if (cart.Inventory.IsEmpty())
            {
                cart.Destination = cart.Origin;
                if (this.ConnectedInventories.ContainsKey(cart.Destination))
                {
                    HexCell neighbor = this.Cell.GetNeighbor(this.ConnectedInventories[cart.Destination].Item1);
                    if (neighbor != null && neighbor.Structure is Road)
                    {
                        this.Carts.Remove(cart);
                        ((Road)neighbor.Structure).AddCart(cart);
                    }
                }
                //No connection to origin anymore -> Delete Cart and add the cart at origin
                else
                {
                    this.Carts.Remove(cart);
                    cart.Origin.AddCart(cart);
                }
                return true;
            }
            else
            {
                // transfer overflow immediately back to origin
                cart.Inventory.MoveAllInto(cart.Origin.Inventory);
            }
            return false;
        }

        private KeyValuePair<InventoryBuilding, Tuple<HexDirection, int, int>> FindDestination()
        {
            //First check if there is a building which has an empty ressource which can be send.
            foreach (KeyValuePair<InventoryBuilding, Tuple<HexDirection, int, int>> kvp in this.ConnectedInventories)
            {
                InventoryBuilding possibleDestination = kvp.Key;
                foreach (KeyValuePair<RessourceType, bool> kvp2 in AllowedRessources[kvp.Key])
                {
                    if (kvp2.Value)
                    {
                        if (possibleDestination.Inventory.GetRessourceAmount(kvp2.Key) == 0 && possibleDestination.Inventory.AvailableSpace(kvp2.Key) > 0)
                        {
                            return kvp;
                        }
                    }
                }
            }
            //Then check if a building can be found that has space for any fitting ressourceType
            foreach (KeyValuePair<InventoryBuilding, Tuple<HexDirection, int, int>> kvp in this.ConnectedInventories)
            {
                InventoryBuilding possibleDestination = kvp.Key;
                foreach (KeyValuePair<RessourceType, bool> kvp2 in AllowedRessources[kvp.Key])
                {
                    if (kvp2.Value)
                    {
                        if (possibleDestination.Inventory.AvailableSpace(kvp2.Key) > 0)
                        {
                            return kvp;
                        }
                    }
                }
            }
            throw new Exception("No fitting destination found");
        }

        public virtual bool HasEmptyRessource(InventoryBuilding origin)
        {
            foreach (RessourceType ressourceType in origin.Inventory.Outgoing)
            {
                if (this.Inventory.Incoming.Contains(ressourceType))
                {
                    if (this.Inventory.GetRessourceAmount(ressourceType) == 0)
                        return true;
                }
            }
            return false;
        }

        public virtual bool FillCart(Cart cart, InventoryBuilding origin)
        {
            cart.Capacity = this.ConnectedInventories[origin].Item2;
            BuildingInventory destination = this.Inventory;

            bool addedResource = true;
            while (addedResource)
            {
                addedResource = false;
                foreach (RessourceType ressourceType in destination.Incoming)
                {
                    if (
                        cart.AvailableSpace() > 0 // cart has space
                        && origin.Inventory.Outgoing.Contains(ressourceType) && origin.AllowedRessources[this][ressourceType] // allowed transfer
                        && origin.Inventory.GetRessourceAmount(ressourceType) > 0 // present in origin inventory
                        && destination.AvailableSpace(ressourceType) - cart.Inventory.GetRessourceAmount(ressourceType) > 0 // destination has enough space
                        // other carts may end up filling destination, in that case overflow resources will be returned
                    )
                    {
                        addedResource = true;
                        cart.Inventory.AddRessource(ressourceType, 1);
                        origin.Inventory.RemoveRessource(ressourceType, 1);
                    }
                }
            }

            return !cart.Inventory.IsEmpty();
        }

        public void HandleCarts()
        {
            for(int i = 0; i < this.Carts.Count; i++)
            {
                if (this.Carts[i].Origin == this)
                {
                    //SendCart
                    if (TrySendCart(this.Carts[i]))
                        break;
                }
                else
                {
                    if (UnloadCart(this.Carts[i]))
                        break;
                }
            }
            // throw new NotImplementedException();
        }

        public void AddCart(Cart cart)
        {
            this.Carts.Add(cart);
            cart.HasMoved = true;
        }
    }
}
