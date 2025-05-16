using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace Shared.DataTypes
{
    public enum StructureType
    {
        BIG_ROCK, ROCK, SCRUB, TREES, BIG_TREES, FISH, WHEAT
    }
    /// <summary>
    /// Type of Ressources. DO NOT CHANGE THE ORDER! DO NOT FLAG THE ENUM! DO NOT CHANGE INT VALUES!
    /// </summary>
    public enum RessourceType
    {
        [Description("Wood")]
        WOOD,

        [Description("Stone")]
        STONE,

        [Description("Iron")]
        IRON,

        [Description("Coal")]
        COAL,

        [Description("Wheat")]
        WHEAT,

        [Description("Cow")]
        COW,

        [Description("Food")]
        FOOD,

        [Description("Leather")]
        LEATHER,

        [Description("Sword & Shield")]
        SWORD,

        [Description("Bow & Arrows")]
        BOW,

        [Description("Spear")]
        SPEAR,

        [Description("Iron Armor")]
        IRON_ARMOR,

        [Description("Leather Armor")]
        LEATHER_ARMOR,
    }

    public static class RessourceTypeExtension
    {

        public static string ToFriendlyString(this RessourceType ressource)
        {
            string rawName = ressource.ToString();

            // get description attribute
            FieldInfo field = ressource.GetType().GetField(rawName);
            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;

            // name is attribute or raw enum name as default
            return attribute != null ? attribute.Description : rawName;
        }
    }
}
