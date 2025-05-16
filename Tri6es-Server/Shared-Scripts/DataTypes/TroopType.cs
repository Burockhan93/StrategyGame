using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace Shared.DataTypes
{
    public enum TroopType
    {
        [Description("Archer")]
        ARCHER,

        [Description("Knight")]
        KNIGHT,

        [Description("Spearman")]
        SPEARMAN,

        [Description("Scout")]
        SCOUT,

        [Description("Siege Engine")]
        SIEGE_ENGINE,
    }

    public static class TroopTypeExtenstion
    {
        public static string ToFriendlyString(this TroopType troopType)
        {
            string rawName = troopType.ToString();

            // get description attribute
            FieldInfo field = troopType.GetType().GetField(rawName);
            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;

            // name is attribute or raw enum name as default
            return attribute != null ? attribute.Description : rawName;
        }
    }
}
