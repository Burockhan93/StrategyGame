using System.ComponentModel;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Game;
using Shared.Communication;
using System;

namespace Shared.Structures
{
    [Description("Truhe")]
    class Loot : Collectable
    {
        //public override int maxTime => Constants.HoursToGameTicks(3);
        public override CollectableType type => CollectableType.LOOT;
        public int amount = 1;
        Random random = new Random();
        public Loot() : base()
        {

        }
        public Loot(HexCell cell) : base(cell)
        {

        }
        public Loot(HexCell cell, int time) : base(cell, time)
        {

        }
        public override int Collect(Tribe tribe, Player player)
        {
            Double r = random.NextDouble();
            if (r < 0.05)
            {
                player.TroopInventory.AddUnit(TroopType.ARCHER, amount);
            }
            if (r < 0.1)
            {
                tribe.tribeInventory.AddRessource(RessourceType.LEATHER, amount);
            }
            if (r > 0.1 && r < 0.15)
            {
                player.TroopInventory.AddUnit(TroopType.KNIGHT, amount);
            }
            if (r > 0.15 && r < 0.2)
            {
                tribe.tribeInventory.AddRessource(RessourceType.IRON, amount);
            }
            if (r > 0.2 && r < 0.25)
            {
                tribe.tribeInventory.AddRessource(RessourceType.BOW, amount);
            }
            if (r > 0.25 && r < 0.3)
            {
                player.TroopInventory.AddUnit(TroopType.SCOUT, amount);
            }
            if (r > 0.3 && r < 0.35)
            {
                player.TroopInventory.AddUnit(TroopType.SIEGE_ENGINE, amount);
            }
            if (r > 0.35 && r < 0.4)
            {
                player.TroopInventory.AddUnit(TroopType.SPEARMAN, amount);
            }
            if (r > 0.4 && r < 0.5)
            {
                tribe.tribeInventory.AddRessource(RessourceType.FOOD, amount);
            }
            if (r > 0.5 && r > 0.7)
            {
                tribe.tribeInventory.AddRessource(RessourceType.SWORD, amount);
            }
            if (r > 0.7 && r < 0.8)
            {
                tribe.tribeInventory.AddRessource(RessourceType.SPEAR, amount);
            }
            if (r > 0.8 && r < 0.9)
            {
                tribe.tribeInventory.AddRessource(RessourceType.IRON_ARMOR, amount);
            }
            if (r > 0.9)
            {
                tribe.tribeInventory.AddRessource(RessourceType.LEATHER_ARMOR, amount);
            }
            da = 0;
            return amount;
        }
        public override void DoTick()
        {

        }
    }
}