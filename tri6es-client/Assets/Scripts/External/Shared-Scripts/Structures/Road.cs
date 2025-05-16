using System;
using System.Collections.Generic;
using System.Linq;
using Shared.HexGrid;
using UnityEngine;

namespace Shared.Structures
{
    public abstract class Road : Building, ICartHandler
    {
        private const float ELEVATION_THRESHOLD = 2f;

        public Dictionary<int, Dictionary<InventoryBuilding, Tuple<HexDirection, int, int>>> connectedStorages;

        public List<Cart> Carts { get; set; }

        public Road() : base()
        {
            connectedStorages = new Dictionary<int, Dictionary<InventoryBuilding, Tuple<HexDirection, int, int>>>();
            Carts = new List<Cart>();
        }

        public Road(
            HexCell Cell,
            byte Tribe,
            byte Level
        ) : base(Cell, Tribe, Level)
        {
            connectedStorages = new Dictionary<int, Dictionary<InventoryBuilding, Tuple<HexDirection, int, int>>>();
            Carts = new List<Cart>();
        }

        public virtual int GetElevation()
        {
            return this.Cell.Data.Elevation;
        }

        public virtual int GetElevationDifference(HexDirection direction)
        {
            HexCell cell = this.Cell.GetNeighbor(direction);
            if (cell == null)
                return int.MaxValue;

            if (cell.Structure is Road)
                return Mathf.Abs(this.GetElevation() - ((Road)cell.Structure).GetElevation());

            if (cell.Structure is Building)
                return Mathf.Abs(this.GetElevation() - cell.Elevation);

            return int.MaxValue;
        }


        public bool HasAnyConnection()
        {
            for (HexDirection d = HexDirection.NE; d < HexDirection.NW; d++)
            {
                if (HasBuilding(d))
                    return true;
            }
            return false;
        }

        public bool HasRoad(HexDirection direction)
        {
            if (Cell == null)
                return false;
            HexCell neighbor = Cell.GetNeighbor(direction);
            if (neighbor == null)
                return false;
            if (neighbor.Structure != null && neighbor.Structure is Road && this.GetElevationDifference(direction) < ELEVATION_THRESHOLD)
                return true;
            return false;
        }

        public virtual bool HasBuilding(HexDirection direction)
        {
            if (Cell == null)
                return false;
            HexCell neighbor = Cell.GetNeighbor(direction);
            if (neighbor == null)
                return false;
            if (neighbor.Structure != null && neighbor.Structure is Building && this.GetElevationDifference(direction) < ELEVATION_THRESHOLD)
                return true;
            return false;
        }

        public bool HasStraightLine(HexDirection direction)
        {
            if (HasBuilding(direction) && !HasBuilding(direction.Next()) && !HasBuilding(direction.Next().Next()) && HasBuilding(direction.Opposite()))
                return true;
            return false;
        }

        public bool IsSmoothCorner(HexDirection direction)
        {
            if (HasBuilding(direction.Previous()) && !HasBuilding(direction) && HasBuilding(direction.Next()))
                return true;
            return false;
        }

        public bool IsEmpty(HexDirection direction)
        {
            return !(IsSmoothCorner(direction) || HasStraightLine(direction.Previous()) || HasBuilding(direction));
        }

        public override void DoTick()
        {
            HandleCarts();
        }

        private void MoveCarts()
        {
            if (Carts.Count > 0)
            {
                Cart cart = Carts.First();
                if (cart.HasMoved == false)
                {
                    try
                    {
                        HexCell neighbor = this.Cell.GetNeighbor(this.connectedStorages[cart.Destination.Tribe][cart.Destination].Item1);
                        ((ICartHandler)neighbor.Structure).AddCart(cart);
                        this.Carts.Remove(cart);

                    }
                    catch (Exception e)
                    {
                        cart.Destination = cart.Origin;
                    }
                }
            }
        }

        public void HandleCarts()
        {
            MoveCarts();
        }

        public void AddCart(Cart cart)
        {
            this.Carts.Add(cart);
            cart.HasMoved = true;
        }
    }
}
