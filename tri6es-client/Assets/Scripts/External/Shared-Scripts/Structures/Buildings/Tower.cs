using Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.HexGrid;
using UnityEngine;
using Shared.Communication;

namespace Shared.Structures
{
    class Tower : ProtectedBuilding
    {
        public override string description => "The Tower is used to protect your territory.";
        public override byte MaxLevel => 3;
        public override byte[] MaxHealths => new byte[] { 12, 14, 16 };
        public override byte Radius => 5;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {};

        public override Dictionary<RessourceType, int>[] Recipes => new [] {
            new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10 }, { RessourceType.STONE, 5 }, { RessourceType.LEATHER, 5 } },
            new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 20 }, { RessourceType.STONE, 10 }, { RessourceType.LEATHER, 10 } },
            new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 20 }, { RessourceType.STONE, 20 }, { RessourceType.IRON, 10 } }
        };

        public Tower() : base() {}

        public Tower(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            byte health,
            TroopInventory troops
        ) : base(cell, tribe, level, inventory, health, troops) {}
    }
}
