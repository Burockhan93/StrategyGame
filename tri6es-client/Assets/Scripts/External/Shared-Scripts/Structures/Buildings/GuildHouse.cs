using System.Collections.Generic;
using System.ComponentModel;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    using ResourceMap = Dictionary<RessourceType, int>;
    
    [Description("Guild House")]
    public class GuildHouse : Building
    {
        public override string description => "The guild house is needed in order to be able to join a guild.";
        
        public override byte MaxLevel => 3;
        
        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10}, { RessourceType.STONE, 10 }, { RessourceType.IRON, 10 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10}, { RessourceType.STONE, 10 }, { RessourceType.IRON, 10 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10}, { RessourceType.STONE, 10 }, { RessourceType.IRON, 10 } },
                };
                return result;
            }
        }

        // Boost options for level 1 guild.
        public RessourceType BoostBaseResource = RessourceType.COW;
        public static readonly List<RessourceType> BaseResourceTypes = new List<RessourceType>()
        {
            RessourceType.COW,
            RessourceType.COAL,
            RessourceType.WOOD,
            RessourceType.STONE,
            RessourceType.WHEAT,
        };

        public RessourceType BoostAdvancedResource = RessourceType.FOOD;
        public static readonly List<RessourceType> AdvancedResourceTypes = new List<RessourceType>()
        {
            RessourceType.FOOD,
            RessourceType.IRON,
            RessourceType.LEATHER,
        };

        public RessourceType BoostWeapon = RessourceType.BOW;
        public static readonly List<RessourceType> WeaponTypes = new List<RessourceType>()
        {
            RessourceType.BOW,
            RessourceType.SPEAR,
            RessourceType.SWORD,
            RessourceType.IRON_ARMOR,
            RessourceType.LEATHER_ARMOR,
        };

        public GuildHouse()
        {
        }
        
        public GuildHouse(HexCell Cell, byte Tribe, byte Level) : base(Cell, Tribe, Level)
        {
        }
        
        public override void DoTick()
        {
        }

        public void ChangeBoostBaseResource(RessourceType type)
        {
            if(BaseResourceTypes.Contains(type))
            {
                BoostBaseResource = type;
            }
        }

        public void ChangeBoostAdvancedResource(RessourceType type)
        {
            if(AdvancedResourceTypes.Contains(type))
            {
                BoostAdvancedResource = type;
            }
        }

        public void ChangeBoostWeapon(RessourceType type)
        {
            if(WeaponTypes.Contains(type))
            {
                BoostWeapon = type;
            }
        }
    }
}