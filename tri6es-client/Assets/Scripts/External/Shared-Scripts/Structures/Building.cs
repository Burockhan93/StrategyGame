using System.Collections.Generic;
using Shared.HexGrid;
using Shared.DataTypes;

namespace Shared.Structures
{
    public abstract class Building : Structure
    {
        public byte Tribe;
        public byte Level;
        public Ressource CellRessource { get; set;}
        public bool Protected { get; set; }

        public abstract byte MaxLevel { get; }

        public abstract string description { get; }

        public abstract Dictionary<RessourceType, int>[] Recipes { get; }

        public Building() : base()
        {
            this.Tribe = Game.Tribe.ID_NO_TRIBE;
            this.Level = 1;
            this.Protected = false;
        }

        public Building(HexCell Cell, byte Tribe, byte Level) : base(Cell)
        {
            this.Tribe = Tribe;
            this.Level = Level;
            this.Protected = false;
        }

        public virtual void Upgrade()
        {
            if(Level < MaxLevel)
                Level++;
        }

        public virtual void Downgrade()
        {
            if (Level > 0)
                Level--;
        }

        public override bool IsPlaceableAt(HexCell cell)
        {
            // if another tribe has claimed this cell then its not possible to build sth here
            if (cell.GetCurrentTribe() != Tribe )
            {
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

        public bool IsUpgradable()
        {
            return (Level < MaxLevel);
        }
    }
}
