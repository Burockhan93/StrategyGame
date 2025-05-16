using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Game;

namespace Shared.Structures
{
    public abstract class Collectable : Structure
    {
        
       // public abstract int maxTime { get; }
        public abstract CollectableType type { get; }
        public int da = 1;

        public Collectable() : base()
        {

        }
        public Collectable(HexCell cell) : base(cell)
        {
        }
        public Collectable(HexCell cell, int time) : base(cell)
        {

        }
        public abstract int Collect(Tribe tribe, Player player);
        
        
            
        
        
    }
}