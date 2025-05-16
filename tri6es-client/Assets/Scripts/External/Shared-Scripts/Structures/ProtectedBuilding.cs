using System.Collections.Generic;
using UnityEngine;
using Shared.HexGrid;
using System;

namespace Shared.Structures
{
    public abstract class ProtectedBuilding : InventoryBuilding
    {
        public TroopInventory TroopInventory;

        public byte Health;

        public abstract byte Radius { get; }

        public byte MaxHealth => MaxHealths[Level - 1];

        public abstract byte[] MaxHealths { get; }

        public List<Tuple<byte, TroopInventory>> scoutResults = new List<Tuple<byte, TroopInventory>>();

        public ProtectedBuilding() : base()
        {
            // building starts with max health & no troops
            Health = MaxHealth;
            TroopInventory = new TroopInventory();

        }

        public ProtectedBuilding(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            byte health,
            TroopInventory troops
        ) : base(cell, tribe, level, inventory)
        {
            Health = health;
            TroopInventory = troops;
        }

        /// <summary>Returns a list of cells protected by this.</summary>
        public List<HexCell> GetProtectedCells()
        {
            return Cell.GetNeighbors(Radius);
        }

        public void ProtectRadius()
        {
            foreach (HexCell cell in Cell.GetNeighbors(Radius))
            {
                // add to list of protectors
                cell.Protectors.Add(this.Cell.coordinates);

                // set protected flag on buildings
                if (cell.Structure is Building)
                {
                    Building building = (Building) cell.Structure;
                    building.Protected = true;
                }
            }
        }

        public override void DoTick()
        {
            base.DoTick();
            byte prevHealth = Health;
            // regenerate health
            Health = (byte) Mathf.Min(MaxHealth, Health + 1);
            // when the health changes, reevaluate all buildings if they are protected.
            /*if (prevHealth != Health) {
                ProtectRadius(Cell);
            }*/
        }

        public override void Downgrade()
        {
            base.Downgrade();
            // update health
            Health = MaxHealth;
        }

        public override void Upgrade() {
            base.Upgrade();
            ProtectRadius();
        }

        public override bool IsPlaceableAt(HexCell cell){
            //it is possible to build a protectedbuilding on a cell that was not claimed yet.
            int tribe = cell.GetCurrentTribe();
            if (tribe != this.Tribe && tribe != 256){
                return false;
            }
            List<Building> buildings = cell.GetNeighborStructures<Building>(2);
            if (buildings.FindIndex(elem => elem.Tribe != this.Tribe) != -1)
                return false;

            if (cell.Structure != null && typeof(Building).IsAssignableFrom(cell.Structure.GetType()))
            {
                return false;
            }

            return true;
        }
    }
}
