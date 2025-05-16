using System.Collections.Generic;
using System.ComponentModel;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    using ResourceMap = Dictionary<RessourceType, int>;
    
    [Description("Library")]
    public class Library : Building
    {
        public override string description => "The library is used to research new technologies.";

        public override byte MaxLevel => 1;

        public override ResourceMap[] Recipes => new[]
        {
            new ResourceMap {{RessourceType.WOOD, 20}, {RessourceType.STONE, 20}}
        };

        public Library()
        {
        }

        public Library(HexCell Cell, byte Tribe, byte Level) : base(Cell, Tribe, Level)
        {
        }

        public override void DoTick()
        {
        }
    }
}