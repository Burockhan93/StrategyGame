using System.ComponentModel;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    [Description("Assembly Point")]
    public class AssemblyPoint : ProtectedBuilding
    {
        public override string description => "The Assembly Point is used to gather your troops.";
        public override byte MaxLevel => 3;
        public override byte[] MaxHealths => new byte[] { 12, 14, 16 };
        public override byte Radius => 0;
        public int priority; 

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {};

        public override Dictionary<RessourceType, int>[] Recipes => new [] {
            new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 10 } },
            new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 20 }, {RessourceType.STONE, 5} },
            new Dictionary<RessourceType, int>{ {RessourceType.WOOD, 30 }, {RessourceType.STONE, 15} }
        };

        /// <summary>Attack cooldown for this assembly point.</summary>
        public int Cooldown = 0;

        /// <summary>Player-chosen name for this assembly point.</summary>
        public string Name = "";

        public AssemblyPoint() : base() {}

        public AssemblyPoint(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            byte health,
            TroopInventory troops,
            int cooldown,
            string name,
            int priority
        ) : base(cell, tribe, level, inventory, health, troops)
        {
            Cooldown = cooldown;
            Name = name;
            this.priority = priority;
        }

        public override void DoTick()
        {
            if (Cooldown > 0)
                Cooldown--;
            base.DoTick();
        }
    }
}
